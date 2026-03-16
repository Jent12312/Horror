using System;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerVoiceChat : NetworkBehaviour
{
    [Header("Microphone")]
    [SerializeField] private int sampleRate = 16000;
    [SerializeField] private float transmitInterval = 0.08f;
    [SerializeField] private float micGain = 1.5f;
    [SerializeField] private InputReader inputReader;

    [Header("3D Sound")]
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 25f;

    private AudioSource audioSource;
    private string micDevice;
    private AudioClip micClip;
    private int lastMicPos;
    private float sendTimer;
    public bool isMicActive;

    // ── кольцевой буфер приёма (audio thread ↔ main thread) ──
    private float[] ring;
    private int ringW, ringR, ringCount;
    private readonly object ringLock = new object();

    private const int RING_SIZE = 32000; // ~2 с при 16 kHz
    private const int CHUNK_MAX = 900;   // макс. байт на один RPC
    private const int PREBUFFER = 2400;  // 150 мс буфер перед воспроизведением

    private bool buffering = true;
    private float fadeGain;

    // ── предвычисленная константа для µ-law ──
    private static readonly float LOG_MU1 = Mathf.Log(256f); // ln(1+255)

    // ====================================================================
    //  LIFECYCLE
    // ====================================================================

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.dopplerLevel = 0f;

        ring = new float[RING_SIZE];
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // владелец НЕ слышит свой же голос
            audioSource.mute = true;
            InitMic();
            inputReader.ToggleMicEvent += ToggleMic;
        }
        else
        {
            // ВСЕ остальные клиенты — слушатели
            audioSource.mute = false;

            // Потоковый AudioClip: Unity сама вызывает PcmReader
            // из аудио-потока, данные проходят через 3D-спатиализатор
            var clip = AudioClip.Create(
                "VoiceStream",
                sampleRate,       // длина внутреннего буфера (1 с)
                1,                // моно
                sampleRate,       // частота = наш sampleRate
                true,             // stream = true
                PcmReader,        // колбэк чтения
                PcmSetPosition    // колбэк сброса позиции
            );

            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            inputReader.ToggleMicEvent -= ToggleMic;
            if (isMicActive) StopRec();
        }
    }

    // ====================================================================
    //  MICROPHONE  (только у Owner)
    // ====================================================================

    private void InitMic()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("[Voice] Микрофоны не найдены!");
            return;
        }
        // Пробуем найти устройство по имени, а не просто [0]
        micDevice = null;
        foreach (var device in Microphone.devices)
        {
            if (!string.IsNullOrEmpty(device))
            {
                micDevice = device;
                Debug.Log($"[Voice] Найдено устройство: {device}");
                break;
            }
        }

        if (string.IsNullOrEmpty(micDevice))
            Debug.LogError("[Voice] Не удалось инициализировать микрофон");
    }

    private void ToggleMic()
    {
        isMicActive = !isMicActive;
        if (isMicActive) StartRec();
        else StopRec();
        Debug.Log($"[Voice] Mic: {isMicActive}");
    }

    private void StartRec()
    {
        if (string.IsNullOrEmpty(micDevice)) return;
        micClip = Microphone.Start(micDevice, true, 10, sampleRate);
        lastMicPos = 0;
        sendTimer = 0f;
    }

    private void StopRec()
    {
        if (!string.IsNullOrEmpty(micDevice))
            Microphone.End(micDevice);
    }

    // ====================================================================
    //  TRANSMIT  (Owner → Server → All Clients)
    // ====================================================================

    private void Update()
    {
        if (!IsOwner || !isMicActive || micClip == null) return;

        sendTimer += Time.deltaTime;
        if (sendTimer < transmitInterval) return;
        sendTimer = 0f;

        Transmit();
    }

    private void Transmit()
    {
        int pos = Microphone.GetPosition(micDevice);
        if (pos < 0 || pos == lastMicPos) return;

        int count = (pos - lastMicPos + micClip.samples) % micClip.samples;
        if (count == 0) return;

        float[] pcm = new float[count];
        micClip.GetData(pcm, lastMicPos);
        lastMicPos = pos;

        // µ-law кодирование float → byte
        byte[] enc = new byte[count];
        for (int i = 0; i < count; i++)
            enc[i] = MuEncode(Mathf.Clamp(pcm[i] * micGain, -1f, 1f));

        // отправка чанками (≤ 900 байт на RPC, влезает в MTU)
        for (int off = 0; off < enc.Length; off += CHUNK_MAX)
        {
            int len = Mathf.Min(CHUNK_MAX, enc.Length - off);
            byte[] ch = new byte[len];
            Buffer.BlockCopy(enc, off, ch, 0, len);
            SendChunkServerRpc(ch);
        }
    }

    // ====================================================================
    //  NETWORK RPCs
    // ====================================================================

    [ServerRpc(Delivery = RpcDelivery.Unreliable)]
    private void SendChunkServerRpc(byte[] data)
    {
        ReceiveChunkClientRpc(data);
    }

    [ClientRpc(Delivery = RpcDelivery.Unreliable)]
    private void ReceiveChunkClientRpc(byte[] data)
    {
        if (IsOwner) return;                       // себя не слушаем

        lock (ringLock)
        {
            for (int i = 0; i < data.Length; i++)
            {
                ring[ringW] = MuDecode(data[i]);
                ringW = (ringW + 1) % RING_SIZE;

                if (ringCount < RING_SIZE)
                    ringCount++;
                else
                    ringR = (ringR + 1) % RING_SIZE; // переполнение — сдвигаем чтение
            }
        }
    }

    // ====================================================================
    //  PLAYBACK — вызывается Unity из аудио-потока
    //  Данные попадают В AudioClip → проходят 3D spatializer
    // ====================================================================

    private void PcmReader(float[] buf)
    {
        lock (ringLock)
        {
            for (int i = 0; i < buf.Length; i++)
            {
                if (buffering)
                {
                    // копим буфер, чтобы сгладить джиттер сети
                    buf[i] = 0f;
                    if (ringCount >= PREBUFFER)
                    {
                        buffering = false;
                        fadeGain = 0f;
                    }
                }
                else if (ringCount > 0)
                {
                    float s = ring[ringR];
                    ringR = (ringR + 1) % RING_SIZE;
                    ringCount--;

                    // плавное нарастание, чтобы не было щелчка
                    fadeGain = Mathf.Min(fadeGain + 0.005f, 1f);
                    buf[i] = s * fadeGain;
                }
                else
                {
                    // буфер кончился — плавное затухание, затем ребуферизация
                    fadeGain = Mathf.Max(fadeGain - 0.005f, 0f);
                    buf[i] = 0f;
                    if (fadeGain <= 0f) buffering = true;
                }
            }
        }
    }

    private void PcmSetPosition(int newPos) { /* не используется */ }

    // ====================================================================
    //  µ-LAW CODEC  (намного лучше линейного 8-bit)
    //  Encode: sign(x)·ln(1+255|x|)/ln(256)  → byte
    //  Decode: sign(c)·(256^|c| − 1)/255     → float
    // ====================================================================

    private static byte MuEncode(float s)
    {
        float sign = s < 0f ? -1f : 1f;
        float comp = sign * Mathf.Log(1f + 255f * Mathf.Abs(s)) / LOG_MU1;
        return (byte)Mathf.Clamp((comp + 1f) * 127.5f, 0f, 255f);
    }

    private static float MuDecode(byte b)
    {
        float comp = b / 127.5f - 1f;
        float sign = comp < 0f ? -1f : 1f;
        return sign * (Mathf.Pow(256f, Mathf.Abs(comp)) - 1f) / 255f;
    }
}