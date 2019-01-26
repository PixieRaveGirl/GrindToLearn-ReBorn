using Sandbox.ModAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;

namespace Phoera
{
  public delegate void MessageEventCaller(ulong player = 0);

  public delegate void MessageEventCaller<T1>(T1 arg1, ulong player = 0);

  public delegate void MessageEventCaller<T1, T2>(T1 arg1, T2 arg2, ulong player = 0);

  public delegate void MessageEventCaller<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3, ulong player = 0);

  public delegate void MessageEventCaller<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, ulong player = 0);

  public delegate void MessageEventCaller<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5,
    ulong player = 0);

  public delegate void MessageEventCaller<T1, T2, T3, T4, T5, T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6,
    ulong player = 0);

  public delegate void MessageEventCaller<T1, T2, T3, T4, T5, T6, T7>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5,
    T6 arg6, T7 arg7, ulong player = 0);

  public delegate void MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5,
    T6 arg6, T7 arg7, T8 arg8, ulong player = 0);

  public delegate void MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 arg1, T2 arg2, T3 arg3, T4 arg4,
    T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, ulong player = 0);

  public delegate void MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 arg1, T2 arg2, T3 arg3, T4 arg4,
    T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, ulong player = 0);

  public delegate void MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 arg1, T2 arg2, T3 arg3,
    T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, ulong player = 0);

  public delegate void MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 arg1, T2 arg2, T3 arg3,
    T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, ulong player = 0);

  public delegate void MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T1 arg1, T2 arg2,
    T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13,
    ulong player = 0);

  public delegate void MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T1 arg1, T2 arg2,
    T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13,
    T14 arg14, ulong player = 0);

  public delegate void MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T1 arg1,
    T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13,
    T14 arg14, T15 arg15, ulong player = 0);

  public delegate void MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12,
    T13 arg13, T14 arg14, T15 arg15, T16 arg16, ulong player = 0);

  public enum EventOptions
  {
    OnlyToTarget,
    OnlyToServer,
    BroadcastToAll
  }

  public class NetworkHandlerSystem
  {
    public abstract class SyncVar
    {
      protected readonly ulong _key;
      protected NetworkHandlerSystem _nhs;
      public long Entity { get; }

      protected SyncVar(NetworkHandlerSystem nhs, ulong key, long entity)
      {
        _nhs = nhs;
        _key = key;
        Entity = entity;
      }

      public abstract void UpdateValue(byte[] data);
      public abstract void OnRemoved();
      public abstract byte[] GetRawValue();
    }

    public class SyncVar<T> : SyncVar
    {
      ReplicationSystem.Replicator<T> _replicator;
      Action<SyncVar<T>> _valueUpdated, _valueRemoved;
      public T _value;

      public T Value
      {
        get { return _value; }
        set
        {
          if (!value.Equals(_value))
          {
            if (IsMultiplayer)
            {
              var data = GetRawValue();
              _nhs.SendSyncValue(Entity, _key, data);
            }

            _valueUpdated?.Invoke(this);
          }
        }
      }

      public SyncVar(NetworkHandlerSystem nhs, ulong key, long entity, Action<SyncVar<T>> valueUpdated,
        Action<SyncVar<T>> valueRemoved = null, T value = default(T)) : base(nhs, key, entity)
      {
        _valueRemoved = valueRemoved;
        _valueUpdated = valueUpdated;
        _replicator = nhs._replicationSystem.GetReplicator<T>();
        _value = value;
      }

      public override void UpdateValue(byte[] data)
      {
        int offset = 0;
        _value = _replicator.GetObject(data, ref offset);
        _valueUpdated?.Invoke(this);
      }

      public override void OnRemoved()
      {
        _valueRemoved?.Invoke(this);
      }

      public override byte[] GetRawValue()
      {
        var data = new byte[_replicator.RequestLength(_value)];
        int offset = 0;
        _replicator.WriteObject(_value, data, ref offset);
        return data;
      }
    }

    Dictionary<ushort, SyncEventBase> _handlers = new Dictionary<ushort, SyncEventBase>();
    const int minMessageSize = signSize + modIdSize + methodIdSize + senderSize;
    const int maxDataSize = 4096;
    const int bigMessageChunk = maxDataSize - 20;
    const int signSize = sizeof(ushort);
    const int modIdSize = sizeof(uint);
    const int methodIdSize = sizeof(ushort);
    const int senderSize = sizeof(ulong);
    ushort messageHeader = 0xF0E;
    Hasher _hasher = new Hasher();
    const ushort networkMessageId = 0xF0E;
    byte[] _header;
    uint _modId;
    protected ReplicationSystem _replicationSystem = new ReplicationSystem();
    Dictionary<long, Dictionary<ulong, SyncVar>> _vars = new Dictionary<long, Dictionary<ulong, SyncVar>>();

    Dictionary<ulong, Dictionary<ulong, BigMessageCollector>> _bigMessages =
      new Dictionary<ulong, Dictionary<ulong, BigMessageCollector>>();


    ushort methodId;
    static bool _initDone = false;
    private ushort _networkId;
    protected MessageEventCaller<long, ulong, byte[]> SendSyncValue;
    private MessageEventCaller<long, ulong> RequestSyncValue;
    private MessageEventCaller<long> RemoveEntitySyncs;
    private MessageEventCaller<ulong, int, int, byte[]> SendBigMessageChunk;

    class BigMessageCollector
    {
      byte[] data;
      BitArray bits;
      private NetworkHandlerSystem _nhs;

      public ulong Sender { get; private set; }
      public ulong Id { get; private set; }

      public BigMessageCollector(NetworkHandlerSystem nhs, int size, ulong sender, ulong id)
      {
        Sender = sender;
        Id = id;
        _nhs = nhs;
        int amount = size / bigMessageChunk;
        if (size % bigMessageChunk > 0)
          amount++;
        bits = new BitArray(amount, false);
        data = new byte[size];
      }

      public void Update(int chunkNum, byte[] chunkData)
      {
        if ((chunkNum != (bits.Length - 1) && chunkData.Length < bigMessageChunk) || chunkNum >= bits.Length)
          throw new Exception("Damaged chunk recieved");
        Array.Copy(chunkData, 0, data, chunkNum * bigMessageChunk, data.Length);
        bits[chunkNum] = true;
        if (bits.OfType<bool>().All(b => b))
        {
          Dictionary<ulong, BigMessageCollector> msgCol;
          if (_nhs._bigMessages.TryGetValue(Sender, out msgCol))
          {
            msgCol.Remove(Id);
            if (msgCol.Count == 0)
            {
              _nhs._bigMessages.Remove(Sender);
            }
          }

          _nhs.MPMessageHandler(data);
        }
      }
    }

    public NetworkHandlerSystem(uint modId, ushort networkId = networkMessageId)
    {
      _modId = modId;
      _header = new byte[signSize + modIdSize];
      byte[] dat = BitConverter.GetBytes(messageHeader);
      _networkId = networkId;
      Array.Copy(dat, _header, signSize);
      dat = BitConverter.GetBytes(modId);
      Array.Copy(dat, 0, _header, signSize, modIdSize);
    }

    private void BigMessageChunkRecieved(ulong id, int size, int chunkNum, byte[] data, ulong sender)
    {
      Dictionary<ulong, BigMessageCollector> msgCol;
      if (!_bigMessages.TryGetValue(sender, out msgCol))
      {
        _bigMessages[sender] = msgCol = new Dictionary<ulong, BigMessageCollector>();
      }

      BigMessageCollector collector;
      if (!msgCol.TryGetValue(id, out collector))
      {
        msgCol[id] = collector = new BigMessageCollector(this, size, sender, id);
      }

      collector.Update(chunkNum, data);
    }

    private void RemoveEntitySyncsRequested(long entityId, ulong player)
    {
      Dictionary<ulong, SyncVar> syncs;
      if (_vars.TryGetValue(entityId, out syncs))
      {
        foreach (var s in syncs.Values)
        {
          s.OnRemoved();
        }
      }

      _vars.Remove(entityId);
    }

    public SyncVar<T> CreateSync<T>(IMyEntity entity, string key, T value, Action<SyncVar<T>> OnValueUpdated,
      Action<SyncVar<T>> OnRemoved)
    {
      if (IsServer)
        entity.OnClose += Entity_OnClose;
      return CreateSync(entity.EntityId, key, value, OnValueUpdated, OnRemoved);
    }

    private void Entity_OnClose(IMyEntity obj)
    {
      obj.OnClose -= Entity_OnClose;
      RemoveEntitySyncs(obj.EntityId);
    }

    public SyncVar<T> CreateSync<T>(long entity, string key, T value, Action<SyncVar<T>> OnValueUpdated,
      Action<SyncVar<T>> OnRemoved)
    {
      if (!_syncInitDone)
      {
        var tMethodId = methodId;
        methodId = ushort.MaxValue;
        SendSyncValue = Create<long, ulong, byte[]>(SyncValueRecieved, EventOptions.BroadcastToAll);
        RequestSyncValue = Create<long, ulong>(SyncValueRequested, EventOptions.OnlyToServer);
        RemoveEntitySyncs = Create<long>(RemoveEntitySyncsRequested, EventOptions.BroadcastToAll, true);
        SendBigMessageChunk = Create<ulong, int, int, byte[]>(BigMessageChunkRecieved, EventOptions.OnlyToTarget);
        methodId = tMethodId;
        _syncInitDone = true;
      }

      var id = entity;
      var k = _hasher.GetOrCompute(key);
      var res = new SyncVar<T>(this, k, id, OnValueUpdated, OnRemoved, value);
      Dictionary<ulong, SyncVar> syncs;
      if (!_vars.TryGetValue(id, out syncs))
      {
        syncs = new Dictionary<ulong, SyncVar>();
        _vars[id] = syncs;
      }

      syncs[k] = res;
      if (!IsServer)
      {
        RequestSyncValue(res.Entity, k);
      }

      return res;
    }

    private void SyncValueRequested(long entity, ulong key, ulong sender)
    {
      Dictionary<ulong, SyncVar> vars;
      if (_vars.TryGetValue(entity, out vars))
      {
        SyncVar var;
        if (vars.TryGetValue(key, out var))
        {
          SendSyncValue(entity, key, var.GetRawValue(), sender);
        }
      }
    }

    void SyncValueRecieved(long entity, ulong key, byte[] data, ulong sender)
    {
      Dictionary<ulong, SyncVar> vars;
      if (_vars.TryGetValue(entity, out vars))
      {
        SyncVar var;
        if (vars.TryGetValue(key, out var))
        {
          var.UpdateValue(data);
        }
      }
    }

    public void Unload()
    {
      _header = null;
      _handlers.Clear();
      _handlers = null;
      MyAPIGateway.Multiplayer.UnregisterMessageHandler(_networkId, MPMessageHandler);
    }

    byte[] PrepareMessage(int size, out int offset)
    {
      offset = minMessageSize;
      return new byte[size + minMessageSize];
    }

    void SendMessageTo(ushort methodId, byte[] data, ulong target)
    {
      if (data == null || data.Length < minMessageSize)
        return;
      PrepareDataMessage(methodId, data);
      SendMessageTo(data, target);
    }

    ulong chunkId = 0;
    private bool _syncInitDone = false;

    void SendMessageTo(byte[] data, ulong target)
    {
      if (data.Length > maxDataSize)
      {
        chunkId++;
        int num = 0;
        for (int i = 0; i < data.Length; i += bigMessageChunk)
        {
          int size = Math.Min(bigMessageChunk, data.Length - i);
          var tmp = new byte[size];
          Array.Copy(data, i, tmp, 0, size);
          SendBigMessageChunk(chunkId, data.Length, num, tmp, target);
          num++;
        }
      }
      else
      {
        MyAPIGateway.Multiplayer.SendMessageTo(networkMessageId, data, target);
      }
    }

    void SendMessageToServer(ushort methodId, byte[] data)
    {
      if (data == null || data.Length < minMessageSize)
        return;
      PrepareDataMessage(methodId, data);
      SendMessageTo(data, MyAPIGateway.Multiplayer.ServerId);
    }

    void SendMessageToOthers(ushort methodId, byte[] data)
    {
      if (data == null || data.Length < minMessageSize)
        return;
      PrepareDataMessage(methodId, data);
      var players = new List<IMyPlayer>();
      MyAPIGateway.Players.GetPlayers(players);
      var targets = players.Select(p => p.SteamUserId).Union(new ulong[] {MyAPIGateway.Multiplayer.ServerId});
      if (MyAPIGateway.Session.Player != null)
      {
        targets = targets.Except(new ulong[] {MyAPIGateway.Session.Player.SteamUserId});
      }

      foreach (var player in targets)
        SendMessageTo(data, player);
    }

    void PrepareDataMessage(ushort methodId, byte[] data)
    {
      Array.Copy(_header, data, signSize + modIdSize);
      Array.Copy(BitConverter.GetBytes(methodId), 0, data, signSize + modIdSize, methodIdSize);
      Array.Copy(BitConverter.GetBytes(MyAPIGateway.Session.Player?.SteamUserId ?? MyAPIGateway.Multiplayer.MyId), 0,
        data, signSize + modIdSize + methodIdSize, senderSize);
    }

    void MPMessageHandler(byte[] data)
    {
      if (data == null || data.Length < minMessageSize)
        return;
      ushort header = BitConverter.ToUInt16(data, 0);
      if (header != messageHeader)
        return;
      uint modId = BitConverter.ToUInt32(data, signSize);
      if (modId != _modId)
        return;
      header = BitConverter.ToUInt16(data, signSize + modIdSize);
      MyLog.Default.WriteLine($"MPMessageHandler HEADER:{header}");
      foreach (var handlersKey in _handlers.Keys)
      {
        MyLog.Default.WriteLine($"MPMessageHandler key:{handlersKey}");
      }
      MyLog.Default.Flush();
      SyncEventBase handler;
      if (_handlers.TryGetValue(header, out handler))
      {
        if (data.Length >= (minMessageSize + handler.MinMessageSize))
          handler.MessageHandler(data, minMessageSize, data.Length - minMessageSize,
            BitConverter.ToUInt64(data, signSize + modIdSize + methodIdSize));
      }
    }

    public static bool IsDedicaded
    {
      get { return MyAPIGateway.Utilities.IsDedicated; }
    }

    public static bool IsClient
    {
      get
      {
        if (IsMultiplayer && IsDedicaded)
        {
          return !MyAPIGateway.Multiplayer.IsServer;
        }

        return true;
      }
    }

    public static bool IsMultiplayer
    {
      get
      {
        if (MyAPIGateway.Multiplayer != null && MyAPIGateway.Multiplayer.MultiplayerActive)
          return true;
        return false;
      }
    }

    public static bool IsServer
    {
      get
      {
        if (IsMultiplayer)
        {
          return MyAPIGateway.Multiplayer.IsServer;
        }

        return true;
      }
    }

    #region Sync Events

    abstract class SyncEventBase
    {
      protected ushort _methodId;
      public int MinMessageSize { get; protected set; }
      protected EventOptions _eventOptions;
      protected bool _needServer;
      protected NetworkHandlerSystem _nhs;

      public SyncEventBase(NetworkHandlerSystem nhs, ushort methodId, EventOptions eventOptions, int minSize = 0)
      {
        _nhs = nhs;
        _methodId = methodId;
        MinMessageSize = minSize;
        _eventOptions = eventOptions;
      }

      public abstract void MessageHandler(byte[] data, int offset, int size, ulong sender);
    }

    class SyncEvent : SyncEventBase
    {
      MessageEventCaller _handler;

      internal SyncEvent(NetworkHandlerSystem nhs, ushort methodId, EventOptions eventOptions,
        MessageEventCaller handler) : base(nhs, methodId, eventOptions)
      {
        _handler = handler;
      }

      public override void MessageHandler(byte[] data, int offset, int size, ulong sender)
      {
        _handler(sender);
      }

      public void Raise(ulong target = 0)
      {
        int offset;
        var data = _nhs.PrepareMessage(0, out offset);
        switch (_eventOptions)
        {
          case (EventOptions.OnlyToTarget):
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            break;
          case (EventOptions.OnlyToServer):
            _nhs.SendMessageToServer(_methodId, data);
            break;
          default:
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            else
              _nhs.SendMessageToOthers(_methodId, data);
            break;
        }

        _handler();
      }
    }

    class SyncEvent<T1> : SyncEventBase
    {
      MessageEventCaller<T1> _handler;
      ReplicationSystem.Replicator<T1> r1;

      internal SyncEvent(NetworkHandlerSystem nhs, ReplicationSystem _replicationSystem, ushort methodId,
        EventOptions eventOptions, MessageEventCaller<T1> handler) : base(nhs, methodId, eventOptions)
      {
        MinMessageSize = 0;
        _handler = handler;

        r1 = _replicationSystem.GetReplicator<T1>();
        MinMessageSize += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(default(T1));
      }

      public override void MessageHandler(byte[] data, int offset, int size, ulong sender)
      {
        _handler(r1.GetObject(data, ref offset), sender);
      }

      public void Raise(T1 arg1, ulong target = 0)
      {
        int offset;
        int size = 0;
        MyLog.Default.WriteLine($"RAISE {nameof(T1)}");
        MyLog.Default.Flush();
        MyLog.Default.WriteLine($"Raise serialize: {MyAPIGateway.Utilities.SerializeToXML(arg1)}");
        MyLog.Default.Flush();
        size += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(arg1);
        var data = _nhs.PrepareMessage(size, out offset);
        r1.WriteObject(arg1, data, ref offset);

        switch (_eventOptions)
        {
          case (EventOptions.OnlyToTarget):
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            break;
          case (EventOptions.OnlyToServer):
            _nhs.SendMessageToServer(_methodId, data);
            break;
          default:
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            else
              _nhs.SendMessageToOthers(_methodId, data);
            break;
        }
      }
    }

    class SyncEvent<T1, T2> : SyncEventBase
    {
      MessageEventCaller<T1, T2> _handler;
      ReplicationSystem.Replicator<T1> r1;
      ReplicationSystem.Replicator<T2> r2;

      internal SyncEvent(NetworkHandlerSystem nhs, ReplicationSystem _replicationSystem, ushort methodId,
        EventOptions eventOptions, MessageEventCaller<T1, T2> handler) : base(nhs, methodId, eventOptions)
      {
        MinMessageSize = 0;
        _handler = handler;

        r1 = _replicationSystem.GetReplicator<T1>();
        MinMessageSize += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(default(T1));

        r2 = _replicationSystem.GetReplicator<T2>();
        MinMessageSize += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(default(T2));
      }

      public override void MessageHandler(byte[] data, int offset, int size, ulong sender)
      {
        _handler(r1.GetObject(data, ref offset), r2.GetObject(data, ref offset), sender);
      }

      public void Raise(T1 arg1, T2 arg2, ulong target = 0)
      {
        int offset;
        int size = 0;

        size += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(arg1);
        size += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(arg2);
        var data = _nhs.PrepareMessage(size, out offset);
        r1.WriteObject(arg1, data, ref offset);
        r2.WriteObject(arg2, data, ref offset);

        switch (_eventOptions)
        {
          case (EventOptions.OnlyToTarget):
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            break;
          case (EventOptions.OnlyToServer):
            _nhs.SendMessageToServer(_methodId, data);
            break;
          default:
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            else
              _nhs.SendMessageToOthers(_methodId, data);
            break;
        }
      }
    }

    class SyncEvent<T1, T2, T3> : SyncEventBase
    {
      MessageEventCaller<T1, T2, T3> _handler;
      ReplicationSystem.Replicator<T1> r1;
      ReplicationSystem.Replicator<T2> r2;
      ReplicationSystem.Replicator<T3> r3;

      internal SyncEvent(NetworkHandlerSystem nhs, ReplicationSystem _replicationSystem, ushort methodId,
        EventOptions eventOptions, MessageEventCaller<T1, T2, T3> handler) : base(nhs, methodId, eventOptions)
      {
        MinMessageSize = 0;
        _handler = handler;

        r1 = _replicationSystem.GetReplicator<T1>();
        MinMessageSize += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(default(T1));

        r2 = _replicationSystem.GetReplicator<T2>();
        MinMessageSize += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(default(T2));

        r3 = _replicationSystem.GetReplicator<T3>();
        MinMessageSize += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(default(T3));
      }

      public override void MessageHandler(byte[] data, int offset, int size, ulong sender)
      {
        _handler(r1.GetObject(data, ref offset), r2.GetObject(data, ref offset), r3.GetObject(data, ref offset),
          sender);
      }

      public void Raise(T1 arg1, T2 arg2, T3 arg3, ulong target = 0)
      {
        int offset;
        int size = 0;

        size += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(arg1);
        size += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(arg2);
        size += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(arg3);
        var data = _nhs.PrepareMessage(size, out offset);
        r1.WriteObject(arg1, data, ref offset);
        r2.WriteObject(arg2, data, ref offset);
        r3.WriteObject(arg3, data, ref offset);

        switch (_eventOptions)
        {
          case (EventOptions.OnlyToTarget):
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            break;
          case (EventOptions.OnlyToServer):
            _nhs.SendMessageToServer(_methodId, data);
            break;
          default:
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            else
              _nhs.SendMessageToOthers(_methodId, data);
            break;
        }
      }
    }

    class SyncEvent<T1, T2, T3, T4> : SyncEventBase
    {
      MessageEventCaller<T1, T2, T3, T4> _handler;
      ReplicationSystem.Replicator<T1> r1;
      ReplicationSystem.Replicator<T2> r2;
      ReplicationSystem.Replicator<T3> r3;
      ReplicationSystem.Replicator<T4> r4;

      internal SyncEvent(NetworkHandlerSystem nhs, ReplicationSystem _replicationSystem, ushort methodId,
        EventOptions eventOptions, MessageEventCaller<T1, T2, T3, T4> handler) : base(nhs, methodId, eventOptions)
      {
        MinMessageSize = 0;
        _handler = handler;

        r1 = _replicationSystem.GetReplicator<T1>();
        MinMessageSize += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(default(T1));

        r2 = _replicationSystem.GetReplicator<T2>();
        MinMessageSize += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(default(T2));

        r3 = _replicationSystem.GetReplicator<T3>();
        MinMessageSize += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(default(T3));

        r4 = _replicationSystem.GetReplicator<T4>();
        MinMessageSize += r4.HaveConstantLength ? r4.ConstantLength : r4.RequestLength(default(T4));
      }

      public override void MessageHandler(byte[] data, int offset, int size, ulong sender)
      {
        _handler(r1.GetObject(data, ref offset), r2.GetObject(data, ref offset), r3.GetObject(data, ref offset),
          r4.GetObject(data, ref offset), sender);
      }

      public void Raise(T1 arg1, T2 arg2, T3 arg3, T4 arg4, ulong target = 0)
      {
        int offset;
        int size = 0;

        size += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(arg1);
        size += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(arg2);
        size += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(arg3);
        size += r4.HaveConstantLength ? r4.ConstantLength : r4.RequestLength(arg4);
        var data = _nhs.PrepareMessage(size, out offset);
        r1.WriteObject(arg1, data, ref offset);
        r2.WriteObject(arg2, data, ref offset);
        r3.WriteObject(arg3, data, ref offset);
        r4.WriteObject(arg4, data, ref offset);

        switch (_eventOptions)
        {
          case (EventOptions.OnlyToTarget):
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            break;
          case (EventOptions.OnlyToServer):
            _nhs.SendMessageToServer(_methodId, data);
            break;
          default:
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            else
              _nhs.SendMessageToOthers(_methodId, data);
            break;
        }
      }
    }

    class SyncEvent<T1, T2, T3, T4, T5> : SyncEventBase
    {
      MessageEventCaller<T1, T2, T3, T4, T5> _handler;
      ReplicationSystem.Replicator<T1> r1;
      ReplicationSystem.Replicator<T2> r2;
      ReplicationSystem.Replicator<T3> r3;
      ReplicationSystem.Replicator<T4> r4;
      ReplicationSystem.Replicator<T5> r5;

      internal SyncEvent(NetworkHandlerSystem nhs, ReplicationSystem _replicationSystem, ushort methodId,
        EventOptions eventOptions, MessageEventCaller<T1, T2, T3, T4, T5> handler) : base(nhs, methodId, eventOptions)
      {
        MinMessageSize = 0;
        _handler = handler;

        r1 = _replicationSystem.GetReplicator<T1>();
        MinMessageSize += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(default(T1));

        r2 = _replicationSystem.GetReplicator<T2>();
        MinMessageSize += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(default(T2));

        r3 = _replicationSystem.GetReplicator<T3>();
        MinMessageSize += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(default(T3));

        r4 = _replicationSystem.GetReplicator<T4>();
        MinMessageSize += r4.HaveConstantLength ? r4.ConstantLength : r4.RequestLength(default(T4));

        r5 = _replicationSystem.GetReplicator<T5>();
        MinMessageSize += r5.HaveConstantLength ? r5.ConstantLength : r5.RequestLength(default(T5));
      }

      public override void MessageHandler(byte[] data, int offset, int size, ulong sender)
      {
        _handler(r1.GetObject(data, ref offset), r2.GetObject(data, ref offset), r3.GetObject(data, ref offset),
          r4.GetObject(data, ref offset), r5.GetObject(data, ref offset), sender);
      }

      public void Raise(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, ulong target = 0)
      {
        int offset;
        int size = 0;

        size += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(arg1);
        size += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(arg2);
        size += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(arg3);
        size += r4.HaveConstantLength ? r4.ConstantLength : r4.RequestLength(arg4);
        size += r5.HaveConstantLength ? r5.ConstantLength : r5.RequestLength(arg5);
        var data = _nhs.PrepareMessage(size, out offset);
        r1.WriteObject(arg1, data, ref offset);
        r2.WriteObject(arg2, data, ref offset);
        r3.WriteObject(arg3, data, ref offset);
        r4.WriteObject(arg4, data, ref offset);
        r5.WriteObject(arg5, data, ref offset);

        switch (_eventOptions)
        {
          case (EventOptions.OnlyToTarget):
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            break;
          case (EventOptions.OnlyToServer):
            _nhs.SendMessageToServer(_methodId, data);
            break;
          default:
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            else
              _nhs.SendMessageToOthers(_methodId, data);
            break;
        }
      }
    }

    class SyncEvent<T1, T2, T3, T4, T5, T6> : SyncEventBase
    {
      MessageEventCaller<T1, T2, T3, T4, T5, T6> _handler;
      ReplicationSystem.Replicator<T1> r1;
      ReplicationSystem.Replicator<T2> r2;
      ReplicationSystem.Replicator<T3> r3;
      ReplicationSystem.Replicator<T4> r4;
      ReplicationSystem.Replicator<T5> r5;
      ReplicationSystem.Replicator<T6> r6;

      internal SyncEvent(NetworkHandlerSystem nhs, ReplicationSystem _replicationSystem, ushort methodId,
        EventOptions eventOptions, MessageEventCaller<T1, T2, T3, T4, T5, T6> handler) : base(nhs, methodId,
        eventOptions)
      {
        MinMessageSize = 0;
        _handler = handler;

        r1 = _replicationSystem.GetReplicator<T1>();
        MinMessageSize += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(default(T1));

        r2 = _replicationSystem.GetReplicator<T2>();
        MinMessageSize += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(default(T2));

        r3 = _replicationSystem.GetReplicator<T3>();
        MinMessageSize += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(default(T3));

        r4 = _replicationSystem.GetReplicator<T4>();
        MinMessageSize += r4.HaveConstantLength ? r4.ConstantLength : r4.RequestLength(default(T4));

        r5 = _replicationSystem.GetReplicator<T5>();
        MinMessageSize += r5.HaveConstantLength ? r5.ConstantLength : r5.RequestLength(default(T5));

        r6 = _replicationSystem.GetReplicator<T6>();
        MinMessageSize += r6.HaveConstantLength ? r6.ConstantLength : r6.RequestLength(default(T6));
      }

      public override void MessageHandler(byte[] data, int offset, int size, ulong sender)
      {
        _handler(r1.GetObject(data, ref offset), r2.GetObject(data, ref offset), r3.GetObject(data, ref offset),
          r4.GetObject(data, ref offset), r5.GetObject(data, ref offset), r6.GetObject(data, ref offset), sender);
      }

      public void Raise(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, ulong target = 0)
      {
        int offset;
        int size = 0;

        size += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(arg1);
        size += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(arg2);
        size += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(arg3);
        size += r4.HaveConstantLength ? r4.ConstantLength : r4.RequestLength(arg4);
        size += r5.HaveConstantLength ? r5.ConstantLength : r5.RequestLength(arg5);
        size += r6.HaveConstantLength ? r6.ConstantLength : r6.RequestLength(arg6);
        var data = _nhs.PrepareMessage(size, out offset);
        r1.WriteObject(arg1, data, ref offset);
        r2.WriteObject(arg2, data, ref offset);
        r3.WriteObject(arg3, data, ref offset);
        r4.WriteObject(arg4, data, ref offset);
        r5.WriteObject(arg5, data, ref offset);
        r6.WriteObject(arg6, data, ref offset);

        switch (_eventOptions)
        {
          case (EventOptions.OnlyToTarget):
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            break;
          case (EventOptions.OnlyToServer):
            _nhs.SendMessageToServer(_methodId, data);
            break;
          default:
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            else
              _nhs.SendMessageToOthers(_methodId, data);
            break;
        }
      }
    }

    class SyncEvent<T1, T2, T3, T4, T5, T6, T7> : SyncEventBase
    {
      MessageEventCaller<T1, T2, T3, T4, T5, T6, T7> _handler;
      ReplicationSystem.Replicator<T1> r1;
      ReplicationSystem.Replicator<T2> r2;
      ReplicationSystem.Replicator<T3> r3;
      ReplicationSystem.Replicator<T4> r4;
      ReplicationSystem.Replicator<T5> r5;
      ReplicationSystem.Replicator<T6> r6;
      ReplicationSystem.Replicator<T7> r7;

      internal SyncEvent(NetworkHandlerSystem nhs, ReplicationSystem _replicationSystem, ushort methodId,
        EventOptions eventOptions, MessageEventCaller<T1, T2, T3, T4, T5, T6, T7> handler) : base(nhs, methodId,
        eventOptions)
      {
        MinMessageSize = 0;
        _handler = handler;

        r1 = _replicationSystem.GetReplicator<T1>();
        MinMessageSize += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(default(T1));

        r2 = _replicationSystem.GetReplicator<T2>();
        MinMessageSize += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(default(T2));

        r3 = _replicationSystem.GetReplicator<T3>();
        MinMessageSize += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(default(T3));

        r4 = _replicationSystem.GetReplicator<T4>();
        MinMessageSize += r4.HaveConstantLength ? r4.ConstantLength : r4.RequestLength(default(T4));

        r5 = _replicationSystem.GetReplicator<T5>();
        MinMessageSize += r5.HaveConstantLength ? r5.ConstantLength : r5.RequestLength(default(T5));

        r6 = _replicationSystem.GetReplicator<T6>();
        MinMessageSize += r6.HaveConstantLength ? r6.ConstantLength : r6.RequestLength(default(T6));

        r7 = _replicationSystem.GetReplicator<T7>();
        MinMessageSize += r7.HaveConstantLength ? r7.ConstantLength : r7.RequestLength(default(T7));
      }

      public override void MessageHandler(byte[] data, int offset, int size, ulong sender)
      {
        _handler(r1.GetObject(data, ref offset), r2.GetObject(data, ref offset), r3.GetObject(data, ref offset),
          r4.GetObject(data, ref offset), r5.GetObject(data, ref offset), r6.GetObject(data, ref offset),
          r7.GetObject(data, ref offset), sender);
      }

      public void Raise(
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, ulong target = 0)
      {
        int offset;
        int size = 0;

        size += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(arg1);
        size += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(arg2);
        size += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(arg3);
        size += r4.HaveConstantLength ? r4.ConstantLength : r4.RequestLength(arg4);
        size += r5.HaveConstantLength ? r5.ConstantLength : r5.RequestLength(arg5);
        size += r6.HaveConstantLength ? r6.ConstantLength : r6.RequestLength(arg6);
        size += r7.HaveConstantLength ? r7.ConstantLength : r7.RequestLength(arg7);
        var data = _nhs.PrepareMessage(size, out offset);
        r1.WriteObject(arg1, data, ref offset);
        r2.WriteObject(arg2, data, ref offset);
        r3.WriteObject(arg3, data, ref offset);
        r4.WriteObject(arg4, data, ref offset);
        r5.WriteObject(arg5, data, ref offset);
        r6.WriteObject(arg6, data, ref offset);
        r7.WriteObject(arg7, data, ref offset);

        switch (_eventOptions)
        {
          case (EventOptions.OnlyToTarget):
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            break;
          case (EventOptions.OnlyToServer):
            _nhs.SendMessageToServer(_methodId, data);
            break;
          default:
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            else
              _nhs.SendMessageToOthers(_methodId, data);
            break;
        }
      }
    }

    class SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8> : SyncEventBase
    {
      MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8> _handler;
      ReplicationSystem.Replicator<T1> r1;
      ReplicationSystem.Replicator<T2> r2;
      ReplicationSystem.Replicator<T3> r3;
      ReplicationSystem.Replicator<T4> r4;
      ReplicationSystem.Replicator<T5> r5;
      ReplicationSystem.Replicator<T6> r6;
      ReplicationSystem.Replicator<T7> r7;
      ReplicationSystem.Replicator<T8> r8;

      internal SyncEvent(NetworkHandlerSystem nhs, ReplicationSystem _replicationSystem, ushort methodId,
        EventOptions eventOptions, MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8> handler) : base(nhs, methodId,
        eventOptions)
      {
        MinMessageSize = 0;
        _handler = handler;

        r1 = _replicationSystem.GetReplicator<T1>();
        MinMessageSize += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(default(T1));

        r2 = _replicationSystem.GetReplicator<T2>();
        MinMessageSize += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(default(T2));

        r3 = _replicationSystem.GetReplicator<T3>();
        MinMessageSize += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(default(T3));

        r4 = _replicationSystem.GetReplicator<T4>();
        MinMessageSize += r4.HaveConstantLength ? r4.ConstantLength : r4.RequestLength(default(T4));

        r5 = _replicationSystem.GetReplicator<T5>();
        MinMessageSize += r5.HaveConstantLength ? r5.ConstantLength : r5.RequestLength(default(T5));

        r6 = _replicationSystem.GetReplicator<T6>();
        MinMessageSize += r6.HaveConstantLength ? r6.ConstantLength : r6.RequestLength(default(T6));

        r7 = _replicationSystem.GetReplicator<T7>();
        MinMessageSize += r7.HaveConstantLength ? r7.ConstantLength : r7.RequestLength(default(T7));

        r8 = _replicationSystem.GetReplicator<T8>();
        MinMessageSize += r8.HaveConstantLength ? r8.ConstantLength : r8.RequestLength(default(T8));
      }

      public override void MessageHandler(byte[] data, int offset, int size, ulong sender)
      {
        _handler(r1.GetObject(data, ref offset), r2.GetObject(data, ref offset), r3.GetObject(data, ref offset),
          r4.GetObject(data, ref offset), r5.GetObject(data, ref offset), r6.GetObject(data, ref offset),
          r7.GetObject(data, ref offset), r8.GetObject(data, ref offset), sender);
      }

      public void Raise(
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, ulong target = 0)
      {
        int offset;
        int size = 0;

        size += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(arg1);
        size += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(arg2);
        size += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(arg3);
        size += r4.HaveConstantLength ? r4.ConstantLength : r4.RequestLength(arg4);
        size += r5.HaveConstantLength ? r5.ConstantLength : r5.RequestLength(arg5);
        size += r6.HaveConstantLength ? r6.ConstantLength : r6.RequestLength(arg6);
        size += r7.HaveConstantLength ? r7.ConstantLength : r7.RequestLength(arg7);
        size += r8.HaveConstantLength ? r8.ConstantLength : r8.RequestLength(arg8);
        var data = _nhs.PrepareMessage(size, out offset);
        r1.WriteObject(arg1, data, ref offset);
        r2.WriteObject(arg2, data, ref offset);
        r3.WriteObject(arg3, data, ref offset);
        r4.WriteObject(arg4, data, ref offset);
        r5.WriteObject(arg5, data, ref offset);
        r6.WriteObject(arg6, data, ref offset);
        r7.WriteObject(arg7, data, ref offset);
        r8.WriteObject(arg8, data, ref offset);

        switch (_eventOptions)
        {
          case (EventOptions.OnlyToTarget):
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            break;
          case (EventOptions.OnlyToServer):
            _nhs.SendMessageToServer(_methodId, data);
            break;
          default:
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            else
              _nhs.SendMessageToOthers(_methodId, data);
            break;
        }
      }
    }

    class SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9> : SyncEventBase
    {
      MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9> _handler;
      ReplicationSystem.Replicator<T1> r1;
      ReplicationSystem.Replicator<T2> r2;
      ReplicationSystem.Replicator<T3> r3;
      ReplicationSystem.Replicator<T4> r4;
      ReplicationSystem.Replicator<T5> r5;
      ReplicationSystem.Replicator<T6> r6;
      ReplicationSystem.Replicator<T7> r7;
      ReplicationSystem.Replicator<T8> r8;
      ReplicationSystem.Replicator<T9> r9;

      internal SyncEvent(NetworkHandlerSystem nhs, ReplicationSystem _replicationSystem, ushort methodId,
        EventOptions eventOptions, MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9> handler) : base(nhs, methodId,
        eventOptions)
      {
        MinMessageSize = 0;
        _handler = handler;

        r1 = _replicationSystem.GetReplicator<T1>();
        MinMessageSize += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(default(T1));

        r2 = _replicationSystem.GetReplicator<T2>();
        MinMessageSize += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(default(T2));

        r3 = _replicationSystem.GetReplicator<T3>();
        MinMessageSize += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(default(T3));

        r4 = _replicationSystem.GetReplicator<T4>();
        MinMessageSize += r4.HaveConstantLength ? r4.ConstantLength : r4.RequestLength(default(T4));

        r5 = _replicationSystem.GetReplicator<T5>();
        MinMessageSize += r5.HaveConstantLength ? r5.ConstantLength : r5.RequestLength(default(T5));

        r6 = _replicationSystem.GetReplicator<T6>();
        MinMessageSize += r6.HaveConstantLength ? r6.ConstantLength : r6.RequestLength(default(T6));

        r7 = _replicationSystem.GetReplicator<T7>();
        MinMessageSize += r7.HaveConstantLength ? r7.ConstantLength : r7.RequestLength(default(T7));

        r8 = _replicationSystem.GetReplicator<T8>();
        MinMessageSize += r8.HaveConstantLength ? r8.ConstantLength : r8.RequestLength(default(T8));

        r9 = _replicationSystem.GetReplicator<T9>();
        MinMessageSize += r9.HaveConstantLength ? r9.ConstantLength : r9.RequestLength(default(T9));
      }

      public override void MessageHandler(byte[] data, int offset, int size, ulong sender)
      {
        _handler(r1.GetObject(data, ref offset), r2.GetObject(data, ref offset), r3.GetObject(data, ref offset),
          r4.GetObject(data, ref offset), r5.GetObject(data, ref offset), r6.GetObject(data, ref offset),
          r7.GetObject(data, ref offset), r8.GetObject(data, ref offset), r9.GetObject(data, ref offset), sender);
      }

      public void Raise(
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, ulong target = 0)
      {
        int offset;
        int size = 0;

        size += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(arg1);
        size += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(arg2);
        size += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(arg3);
        size += r4.HaveConstantLength ? r4.ConstantLength : r4.RequestLength(arg4);
        size += r5.HaveConstantLength ? r5.ConstantLength : r5.RequestLength(arg5);
        size += r6.HaveConstantLength ? r6.ConstantLength : r6.RequestLength(arg6);
        size += r7.HaveConstantLength ? r7.ConstantLength : r7.RequestLength(arg7);
        size += r8.HaveConstantLength ? r8.ConstantLength : r8.RequestLength(arg8);
        size += r9.HaveConstantLength ? r9.ConstantLength : r9.RequestLength(arg9);
        var data = _nhs.PrepareMessage(size, out offset);
        r1.WriteObject(arg1, data, ref offset);
        r2.WriteObject(arg2, data, ref offset);
        r3.WriteObject(arg3, data, ref offset);
        r4.WriteObject(arg4, data, ref offset);
        r5.WriteObject(arg5, data, ref offset);
        r6.WriteObject(arg6, data, ref offset);
        r7.WriteObject(arg7, data, ref offset);
        r8.WriteObject(arg8, data, ref offset);
        r9.WriteObject(arg9, data, ref offset);

        switch (_eventOptions)
        {
          case (EventOptions.OnlyToTarget):
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            break;
          case (EventOptions.OnlyToServer):
            _nhs.SendMessageToServer(_methodId, data);
            break;
          default:
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            else
              _nhs.SendMessageToOthers(_methodId, data);
            break;
        }
      }
    }

    class SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : SyncEventBase
    {
      MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> _handler;
      ReplicationSystem.Replicator<T1> r1;
      ReplicationSystem.Replicator<T2> r2;
      ReplicationSystem.Replicator<T3> r3;
      ReplicationSystem.Replicator<T4> r4;
      ReplicationSystem.Replicator<T5> r5;
      ReplicationSystem.Replicator<T6> r6;
      ReplicationSystem.Replicator<T7> r7;
      ReplicationSystem.Replicator<T8> r8;
      ReplicationSystem.Replicator<T9> r9;
      ReplicationSystem.Replicator<T10> r10;

      internal SyncEvent(NetworkHandlerSystem nhs, ReplicationSystem _replicationSystem, ushort methodId,
        EventOptions eventOptions, MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> handler) : base(nhs,
        methodId, eventOptions)
      {
        MinMessageSize = 0;
        _handler = handler;

        r1 = _replicationSystem.GetReplicator<T1>();
        MinMessageSize += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(default(T1));

        r2 = _replicationSystem.GetReplicator<T2>();
        MinMessageSize += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(default(T2));

        r3 = _replicationSystem.GetReplicator<T3>();
        MinMessageSize += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(default(T3));

        r4 = _replicationSystem.GetReplicator<T4>();
        MinMessageSize += r4.HaveConstantLength ? r4.ConstantLength : r4.RequestLength(default(T4));

        r5 = _replicationSystem.GetReplicator<T5>();
        MinMessageSize += r5.HaveConstantLength ? r5.ConstantLength : r5.RequestLength(default(T5));

        r6 = _replicationSystem.GetReplicator<T6>();
        MinMessageSize += r6.HaveConstantLength ? r6.ConstantLength : r6.RequestLength(default(T6));

        r7 = _replicationSystem.GetReplicator<T7>();
        MinMessageSize += r7.HaveConstantLength ? r7.ConstantLength : r7.RequestLength(default(T7));

        r8 = _replicationSystem.GetReplicator<T8>();
        MinMessageSize += r8.HaveConstantLength ? r8.ConstantLength : r8.RequestLength(default(T8));

        r9 = _replicationSystem.GetReplicator<T9>();
        MinMessageSize += r9.HaveConstantLength ? r9.ConstantLength : r9.RequestLength(default(T9));

        r10 = _replicationSystem.GetReplicator<T10>();
        MinMessageSize += r10.HaveConstantLength ? r10.ConstantLength : r10.RequestLength(default(T10));
      }

      public override void MessageHandler(byte[] data, int offset, int size, ulong sender)
      {
        _handler(r1.GetObject(data, ref offset), r2.GetObject(data, ref offset), r3.GetObject(data, ref offset),
          r4.GetObject(data, ref offset), r5.GetObject(data, ref offset), r6.GetObject(data, ref offset),
          r7.GetObject(data, ref offset), r8.GetObject(data, ref offset), r9.GetObject(data, ref offset),
          r10.GetObject(data, ref offset), sender);
      }

      public void Raise(
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, ulong target = 0)
      {
        int offset;
        int size = 0;

        size += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(arg1);
        size += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(arg2);
        size += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(arg3);
        size += r4.HaveConstantLength ? r4.ConstantLength : r4.RequestLength(arg4);
        size += r5.HaveConstantLength ? r5.ConstantLength : r5.RequestLength(arg5);
        size += r6.HaveConstantLength ? r6.ConstantLength : r6.RequestLength(arg6);
        size += r7.HaveConstantLength ? r7.ConstantLength : r7.RequestLength(arg7);
        size += r8.HaveConstantLength ? r8.ConstantLength : r8.RequestLength(arg8);
        size += r9.HaveConstantLength ? r9.ConstantLength : r9.RequestLength(arg9);
        size += r10.HaveConstantLength ? r10.ConstantLength : r10.RequestLength(arg10);
        var data = _nhs.PrepareMessage(size, out offset);
        r1.WriteObject(arg1, data, ref offset);
        r2.WriteObject(arg2, data, ref offset);
        r3.WriteObject(arg3, data, ref offset);
        r4.WriteObject(arg4, data, ref offset);
        r5.WriteObject(arg5, data, ref offset);
        r6.WriteObject(arg6, data, ref offset);
        r7.WriteObject(arg7, data, ref offset);
        r8.WriteObject(arg8, data, ref offset);
        r9.WriteObject(arg9, data, ref offset);
        r10.WriteObject(arg10, data, ref offset);

        switch (_eventOptions)
        {
          case (EventOptions.OnlyToTarget):
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            break;
          case (EventOptions.OnlyToServer):
            _nhs.SendMessageToServer(_methodId, data);
            break;
          default:
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            else
              _nhs.SendMessageToOthers(_methodId, data);
            break;
        }
      }
    }

    class SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : SyncEventBase
    {
      MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> _handler;
      ReplicationSystem.Replicator<T1> r1;
      ReplicationSystem.Replicator<T2> r2;
      ReplicationSystem.Replicator<T3> r3;
      ReplicationSystem.Replicator<T4> r4;
      ReplicationSystem.Replicator<T5> r5;
      ReplicationSystem.Replicator<T6> r6;
      ReplicationSystem.Replicator<T7> r7;
      ReplicationSystem.Replicator<T8> r8;
      ReplicationSystem.Replicator<T9> r9;
      ReplicationSystem.Replicator<T10> r10;
      ReplicationSystem.Replicator<T11> r11;

      internal SyncEvent(NetworkHandlerSystem nhs, ReplicationSystem _replicationSystem, ushort methodId,
        EventOptions eventOptions, MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> handler) : base(nhs,
        methodId, eventOptions)
      {
        MinMessageSize = 0;
        _handler = handler;

        r1 = _replicationSystem.GetReplicator<T1>();
        MinMessageSize += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(default(T1));

        r2 = _replicationSystem.GetReplicator<T2>();
        MinMessageSize += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(default(T2));

        r3 = _replicationSystem.GetReplicator<T3>();
        MinMessageSize += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(default(T3));

        r4 = _replicationSystem.GetReplicator<T4>();
        MinMessageSize += r4.HaveConstantLength ? r4.ConstantLength : r4.RequestLength(default(T4));

        r5 = _replicationSystem.GetReplicator<T5>();
        MinMessageSize += r5.HaveConstantLength ? r5.ConstantLength : r5.RequestLength(default(T5));

        r6 = _replicationSystem.GetReplicator<T6>();
        MinMessageSize += r6.HaveConstantLength ? r6.ConstantLength : r6.RequestLength(default(T6));

        r7 = _replicationSystem.GetReplicator<T7>();
        MinMessageSize += r7.HaveConstantLength ? r7.ConstantLength : r7.RequestLength(default(T7));

        r8 = _replicationSystem.GetReplicator<T8>();
        MinMessageSize += r8.HaveConstantLength ? r8.ConstantLength : r8.RequestLength(default(T8));

        r9 = _replicationSystem.GetReplicator<T9>();
        MinMessageSize += r9.HaveConstantLength ? r9.ConstantLength : r9.RequestLength(default(T9));

        r10 = _replicationSystem.GetReplicator<T10>();
        MinMessageSize += r10.HaveConstantLength ? r10.ConstantLength : r10.RequestLength(default(T10));

        r11 = _replicationSystem.GetReplicator<T11>();
        MinMessageSize += r11.HaveConstantLength ? r11.ConstantLength : r11.RequestLength(default(T11));
      }

      public override void MessageHandler(byte[] data, int offset, int size, ulong sender)
      {
        _handler(r1.GetObject(data, ref offset), r2.GetObject(data, ref offset), r3.GetObject(data, ref offset),
          r4.GetObject(data, ref offset), r5.GetObject(data, ref offset), r6.GetObject(data, ref offset),
          r7.GetObject(data, ref offset), r8.GetObject(data, ref offset), r9.GetObject(data, ref offset),
          r10.GetObject(data, ref offset), r11.GetObject(data, ref offset), sender);
      }

      public void Raise(
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11,
        ulong target = 0)
      {
        int offset;
        int size = 0;

        size += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(arg1);
        size += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(arg2);
        size += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(arg3);
        size += r4.HaveConstantLength ? r4.ConstantLength : r4.RequestLength(arg4);
        size += r5.HaveConstantLength ? r5.ConstantLength : r5.RequestLength(arg5);
        size += r6.HaveConstantLength ? r6.ConstantLength : r6.RequestLength(arg6);
        size += r7.HaveConstantLength ? r7.ConstantLength : r7.RequestLength(arg7);
        size += r8.HaveConstantLength ? r8.ConstantLength : r8.RequestLength(arg8);
        size += r9.HaveConstantLength ? r9.ConstantLength : r9.RequestLength(arg9);
        size += r10.HaveConstantLength ? r10.ConstantLength : r10.RequestLength(arg10);
        size += r11.HaveConstantLength ? r11.ConstantLength : r11.RequestLength(arg11);
        var data = _nhs.PrepareMessage(size, out offset);
        r1.WriteObject(arg1, data, ref offset);
        r2.WriteObject(arg2, data, ref offset);
        r3.WriteObject(arg3, data, ref offset);
        r4.WriteObject(arg4, data, ref offset);
        r5.WriteObject(arg5, data, ref offset);
        r6.WriteObject(arg6, data, ref offset);
        r7.WriteObject(arg7, data, ref offset);
        r8.WriteObject(arg8, data, ref offset);
        r9.WriteObject(arg9, data, ref offset);
        r10.WriteObject(arg10, data, ref offset);
        r11.WriteObject(arg11, data, ref offset);

        switch (_eventOptions)
        {
          case (EventOptions.OnlyToTarget):
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            break;
          case (EventOptions.OnlyToServer):
            _nhs.SendMessageToServer(_methodId, data);
            break;
          default:
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            else
              _nhs.SendMessageToOthers(_methodId, data);
            _nhs.SendMessageToOthers(_methodId, data);
            break;
        }
      }
    }

    class SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : SyncEventBase
    {
      MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> _handler;
      ReplicationSystem.Replicator<T1> r1;
      ReplicationSystem.Replicator<T2> r2;
      ReplicationSystem.Replicator<T3> r3;
      ReplicationSystem.Replicator<T4> r4;
      ReplicationSystem.Replicator<T5> r5;
      ReplicationSystem.Replicator<T6> r6;
      ReplicationSystem.Replicator<T7> r7;
      ReplicationSystem.Replicator<T8> r8;
      ReplicationSystem.Replicator<T9> r9;
      ReplicationSystem.Replicator<T10> r10;
      ReplicationSystem.Replicator<T11> r11;
      ReplicationSystem.Replicator<T12> r12;

      internal SyncEvent(NetworkHandlerSystem nhs, ReplicationSystem _replicationSystem, ushort methodId,
        EventOptions eventOptions, MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> handler) :
        base(nhs, methodId, eventOptions)
      {
        MinMessageSize = 0;
        _handler = handler;

        r1 = _replicationSystem.GetReplicator<T1>();
        MinMessageSize += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(default(T1));

        r2 = _replicationSystem.GetReplicator<T2>();
        MinMessageSize += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(default(T2));

        r3 = _replicationSystem.GetReplicator<T3>();
        MinMessageSize += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(default(T3));

        r4 = _replicationSystem.GetReplicator<T4>();
        MinMessageSize += r4.HaveConstantLength ? r4.ConstantLength : r4.RequestLength(default(T4));

        r5 = _replicationSystem.GetReplicator<T5>();
        MinMessageSize += r5.HaveConstantLength ? r5.ConstantLength : r5.RequestLength(default(T5));

        r6 = _replicationSystem.GetReplicator<T6>();
        MinMessageSize += r6.HaveConstantLength ? r6.ConstantLength : r6.RequestLength(default(T6));

        r7 = _replicationSystem.GetReplicator<T7>();
        MinMessageSize += r7.HaveConstantLength ? r7.ConstantLength : r7.RequestLength(default(T7));

        r8 = _replicationSystem.GetReplicator<T8>();
        MinMessageSize += r8.HaveConstantLength ? r8.ConstantLength : r8.RequestLength(default(T8));

        r9 = _replicationSystem.GetReplicator<T9>();
        MinMessageSize += r9.HaveConstantLength ? r9.ConstantLength : r9.RequestLength(default(T9));

        r10 = _replicationSystem.GetReplicator<T10>();
        MinMessageSize += r10.HaveConstantLength ? r10.ConstantLength : r10.RequestLength(default(T10));

        r11 = _replicationSystem.GetReplicator<T11>();
        MinMessageSize += r11.HaveConstantLength ? r11.ConstantLength : r11.RequestLength(default(T11));

        r12 = _replicationSystem.GetReplicator<T12>();
        MinMessageSize += r12.HaveConstantLength ? r12.ConstantLength : r12.RequestLength(default(T12));
      }

      public override void MessageHandler(byte[] data, int offset, int size, ulong sender)
      {
        _handler(r1.GetObject(data, ref offset), r2.GetObject(data, ref offset), r3.GetObject(data, ref offset),
          r4.GetObject(data, ref offset), r5.GetObject(data, ref offset), r6.GetObject(data, ref offset),
          r7.GetObject(data, ref offset), r8.GetObject(data, ref offset), r9.GetObject(data, ref offset),
          r10.GetObject(data, ref offset), r11.GetObject(data, ref offset), r12.GetObject(data, ref offset), sender);
      }

      public void Raise(
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11,
        T12 arg12, ulong target = 0)
      {
        int offset;
        int size = 0;

        size += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(arg1);
        size += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(arg2);
        size += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(arg3);
        size += r4.HaveConstantLength ? r4.ConstantLength : r4.RequestLength(arg4);
        size += r5.HaveConstantLength ? r5.ConstantLength : r5.RequestLength(arg5);
        size += r6.HaveConstantLength ? r6.ConstantLength : r6.RequestLength(arg6);
        size += r7.HaveConstantLength ? r7.ConstantLength : r7.RequestLength(arg7);
        size += r8.HaveConstantLength ? r8.ConstantLength : r8.RequestLength(arg8);
        size += r9.HaveConstantLength ? r9.ConstantLength : r9.RequestLength(arg9);
        size += r10.HaveConstantLength ? r10.ConstantLength : r10.RequestLength(arg10);
        size += r11.HaveConstantLength ? r11.ConstantLength : r11.RequestLength(arg11);
        size += r12.HaveConstantLength ? r12.ConstantLength : r12.RequestLength(arg12);
        var data = _nhs.PrepareMessage(size, out offset);
        r1.WriteObject(arg1, data, ref offset);
        r2.WriteObject(arg2, data, ref offset);
        r3.WriteObject(arg3, data, ref offset);
        r4.WriteObject(arg4, data, ref offset);
        r5.WriteObject(arg5, data, ref offset);
        r6.WriteObject(arg6, data, ref offset);
        r7.WriteObject(arg7, data, ref offset);
        r8.WriteObject(arg8, data, ref offset);
        r9.WriteObject(arg9, data, ref offset);
        r10.WriteObject(arg10, data, ref offset);
        r11.WriteObject(arg11, data, ref offset);
        r12.WriteObject(arg12, data, ref offset);

        switch (_eventOptions)
        {
          case (EventOptions.OnlyToTarget):
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            break;
          case (EventOptions.OnlyToServer):
            _nhs.SendMessageToServer(_methodId, data);
            break;
          default:
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            else
              _nhs.SendMessageToOthers(_methodId, data);
            break;
        }
      }
    }

    class SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : SyncEventBase
    {
      MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> _handler;
      ReplicationSystem.Replicator<T1> r1;
      ReplicationSystem.Replicator<T2> r2;
      ReplicationSystem.Replicator<T3> r3;
      ReplicationSystem.Replicator<T4> r4;
      ReplicationSystem.Replicator<T5> r5;
      ReplicationSystem.Replicator<T6> r6;
      ReplicationSystem.Replicator<T7> r7;
      ReplicationSystem.Replicator<T8> r8;
      ReplicationSystem.Replicator<T9> r9;
      ReplicationSystem.Replicator<T10> r10;
      ReplicationSystem.Replicator<T11> r11;
      ReplicationSystem.Replicator<T12> r12;
      ReplicationSystem.Replicator<T13> r13;

      internal SyncEvent(NetworkHandlerSystem nhs, ReplicationSystem _replicationSystem, ushort methodId,
        EventOptions eventOptions, MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> handler) :
        base(nhs, methodId, eventOptions)
      {
        MinMessageSize = 0;
        _handler = handler;

        r1 = _replicationSystem.GetReplicator<T1>();
        MinMessageSize += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(default(T1));

        r2 = _replicationSystem.GetReplicator<T2>();
        MinMessageSize += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(default(T2));

        r3 = _replicationSystem.GetReplicator<T3>();
        MinMessageSize += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(default(T3));

        r4 = _replicationSystem.GetReplicator<T4>();
        MinMessageSize += r4.HaveConstantLength ? r4.ConstantLength : r4.RequestLength(default(T4));

        r5 = _replicationSystem.GetReplicator<T5>();
        MinMessageSize += r5.HaveConstantLength ? r5.ConstantLength : r5.RequestLength(default(T5));

        r6 = _replicationSystem.GetReplicator<T6>();
        MinMessageSize += r6.HaveConstantLength ? r6.ConstantLength : r6.RequestLength(default(T6));

        r7 = _replicationSystem.GetReplicator<T7>();
        MinMessageSize += r7.HaveConstantLength ? r7.ConstantLength : r7.RequestLength(default(T7));

        r8 = _replicationSystem.GetReplicator<T8>();
        MinMessageSize += r8.HaveConstantLength ? r8.ConstantLength : r8.RequestLength(default(T8));

        r9 = _replicationSystem.GetReplicator<T9>();
        MinMessageSize += r9.HaveConstantLength ? r9.ConstantLength : r9.RequestLength(default(T9));

        r10 = _replicationSystem.GetReplicator<T10>();
        MinMessageSize += r10.HaveConstantLength ? r10.ConstantLength : r10.RequestLength(default(T10));

        r11 = _replicationSystem.GetReplicator<T11>();
        MinMessageSize += r11.HaveConstantLength ? r11.ConstantLength : r11.RequestLength(default(T11));

        r12 = _replicationSystem.GetReplicator<T12>();
        MinMessageSize += r12.HaveConstantLength ? r12.ConstantLength : r12.RequestLength(default(T12));

        r13 = _replicationSystem.GetReplicator<T13>();
        MinMessageSize += r13.HaveConstantLength ? r13.ConstantLength : r13.RequestLength(default(T13));
      }

      public override void MessageHandler(byte[] data, int offset, int size, ulong sender)
      {
        _handler(r1.GetObject(data, ref offset), r2.GetObject(data, ref offset), r3.GetObject(data, ref offset),
          r4.GetObject(data, ref offset), r5.GetObject(data, ref offset), r6.GetObject(data, ref offset),
          r7.GetObject(data, ref offset), r8.GetObject(data, ref offset), r9.GetObject(data, ref offset),
          r10.GetObject(data, ref offset), r11.GetObject(data, ref offset), r12.GetObject(data, ref offset),
          r13.GetObject(data, ref offset), sender);
      }

      public void Raise(
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11,
        T12 arg12, T13 arg13, ulong target = 0)
      {
        int offset;
        int size = 0;

        size += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(arg1);
        size += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(arg2);
        size += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(arg3);
        size += r4.HaveConstantLength ? r4.ConstantLength : r4.RequestLength(arg4);
        size += r5.HaveConstantLength ? r5.ConstantLength : r5.RequestLength(arg5);
        size += r6.HaveConstantLength ? r6.ConstantLength : r6.RequestLength(arg6);
        size += r7.HaveConstantLength ? r7.ConstantLength : r7.RequestLength(arg7);
        size += r8.HaveConstantLength ? r8.ConstantLength : r8.RequestLength(arg8);
        size += r9.HaveConstantLength ? r9.ConstantLength : r9.RequestLength(arg9);
        size += r10.HaveConstantLength ? r10.ConstantLength : r10.RequestLength(arg10);
        size += r11.HaveConstantLength ? r11.ConstantLength : r11.RequestLength(arg11);
        size += r12.HaveConstantLength ? r12.ConstantLength : r12.RequestLength(arg12);
        size += r13.HaveConstantLength ? r13.ConstantLength : r13.RequestLength(arg13);
        var data = _nhs.PrepareMessage(size, out offset);
        r1.WriteObject(arg1, data, ref offset);
        r2.WriteObject(arg2, data, ref offset);
        r3.WriteObject(arg3, data, ref offset);
        r4.WriteObject(arg4, data, ref offset);
        r5.WriteObject(arg5, data, ref offset);
        r6.WriteObject(arg6, data, ref offset);
        r7.WriteObject(arg7, data, ref offset);
        r8.WriteObject(arg8, data, ref offset);
        r9.WriteObject(arg9, data, ref offset);
        r10.WriteObject(arg10, data, ref offset);
        r11.WriteObject(arg11, data, ref offset);
        r12.WriteObject(arg12, data, ref offset);
        r13.WriteObject(arg13, data, ref offset);

        switch (_eventOptions)
        {
          case (EventOptions.OnlyToTarget):
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            break;
          case (EventOptions.OnlyToServer):
            _nhs.SendMessageToServer(_methodId, data);
            break;
          default:
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            else
              _nhs.SendMessageToOthers(_methodId, data);
            break;
        }
      }
    }

    class SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : SyncEventBase
    {
      MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> _handler;
      ReplicationSystem.Replicator<T1> r1;
      ReplicationSystem.Replicator<T2> r2;
      ReplicationSystem.Replicator<T3> r3;
      ReplicationSystem.Replicator<T4> r4;
      ReplicationSystem.Replicator<T5> r5;
      ReplicationSystem.Replicator<T6> r6;
      ReplicationSystem.Replicator<T7> r7;
      ReplicationSystem.Replicator<T8> r8;
      ReplicationSystem.Replicator<T9> r9;
      ReplicationSystem.Replicator<T10> r10;
      ReplicationSystem.Replicator<T11> r11;
      ReplicationSystem.Replicator<T12> r12;
      ReplicationSystem.Replicator<T13> r13;
      ReplicationSystem.Replicator<T14> r14;

      internal SyncEvent(NetworkHandlerSystem nhs, ReplicationSystem _replicationSystem, ushort methodId,
        EventOptions eventOptions,
        MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> handler) : base(nhs, methodId,
        eventOptions)
      {
        MinMessageSize = 0;
        _handler = handler;

        r1 = _replicationSystem.GetReplicator<T1>();
        MinMessageSize += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(default(T1));

        r2 = _replicationSystem.GetReplicator<T2>();
        MinMessageSize += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(default(T2));

        r3 = _replicationSystem.GetReplicator<T3>();
        MinMessageSize += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(default(T3));

        r4 = _replicationSystem.GetReplicator<T4>();
        MinMessageSize += r4.HaveConstantLength ? r4.ConstantLength : r4.RequestLength(default(T4));

        r5 = _replicationSystem.GetReplicator<T5>();
        MinMessageSize += r5.HaveConstantLength ? r5.ConstantLength : r5.RequestLength(default(T5));

        r6 = _replicationSystem.GetReplicator<T6>();
        MinMessageSize += r6.HaveConstantLength ? r6.ConstantLength : r6.RequestLength(default(T6));

        r7 = _replicationSystem.GetReplicator<T7>();
        MinMessageSize += r7.HaveConstantLength ? r7.ConstantLength : r7.RequestLength(default(T7));

        r8 = _replicationSystem.GetReplicator<T8>();
        MinMessageSize += r8.HaveConstantLength ? r8.ConstantLength : r8.RequestLength(default(T8));

        r9 = _replicationSystem.GetReplicator<T9>();
        MinMessageSize += r9.HaveConstantLength ? r9.ConstantLength : r9.RequestLength(default(T9));

        r10 = _replicationSystem.GetReplicator<T10>();
        MinMessageSize += r10.HaveConstantLength ? r10.ConstantLength : r10.RequestLength(default(T10));

        r11 = _replicationSystem.GetReplicator<T11>();
        MinMessageSize += r11.HaveConstantLength ? r11.ConstantLength : r11.RequestLength(default(T11));

        r12 = _replicationSystem.GetReplicator<T12>();
        MinMessageSize += r12.HaveConstantLength ? r12.ConstantLength : r12.RequestLength(default(T12));

        r13 = _replicationSystem.GetReplicator<T13>();
        MinMessageSize += r13.HaveConstantLength ? r13.ConstantLength : r13.RequestLength(default(T13));

        r14 = _replicationSystem.GetReplicator<T14>();
        MinMessageSize += r14.HaveConstantLength ? r14.ConstantLength : r14.RequestLength(default(T14));
      }

      public override void MessageHandler(byte[] data, int offset, int size, ulong sender)
      {
        _handler(r1.GetObject(data, ref offset), r2.GetObject(data, ref offset), r3.GetObject(data, ref offset),
          r4.GetObject(data, ref offset), r5.GetObject(data, ref offset), r6.GetObject(data, ref offset),
          r7.GetObject(data, ref offset), r8.GetObject(data, ref offset), r9.GetObject(data, ref offset),
          r10.GetObject(data, ref offset), r11.GetObject(data, ref offset), r12.GetObject(data, ref offset),
          r13.GetObject(data, ref offset), r14.GetObject(data, ref offset), sender);
      }

      public void Raise(
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11,
        T12 arg12, T13 arg13, T14 arg14, ulong target = 0)
      {
        int offset;
        int size = 0;

        size += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(arg1);
        size += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(arg2);
        size += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(arg3);
        size += r4.HaveConstantLength ? r4.ConstantLength : r4.RequestLength(arg4);
        size += r5.HaveConstantLength ? r5.ConstantLength : r5.RequestLength(arg5);
        size += r6.HaveConstantLength ? r6.ConstantLength : r6.RequestLength(arg6);
        size += r7.HaveConstantLength ? r7.ConstantLength : r7.RequestLength(arg7);
        size += r8.HaveConstantLength ? r8.ConstantLength : r8.RequestLength(arg8);
        size += r9.HaveConstantLength ? r9.ConstantLength : r9.RequestLength(arg9);
        size += r10.HaveConstantLength ? r10.ConstantLength : r10.RequestLength(arg10);
        size += r11.HaveConstantLength ? r11.ConstantLength : r11.RequestLength(arg11);
        size += r12.HaveConstantLength ? r12.ConstantLength : r12.RequestLength(arg12);
        size += r13.HaveConstantLength ? r13.ConstantLength : r13.RequestLength(arg13);
        size += r14.HaveConstantLength ? r14.ConstantLength : r14.RequestLength(arg14);
        var data = _nhs.PrepareMessage(size, out offset);
        r1.WriteObject(arg1, data, ref offset);
        r2.WriteObject(arg2, data, ref offset);
        r3.WriteObject(arg3, data, ref offset);
        r4.WriteObject(arg4, data, ref offset);
        r5.WriteObject(arg5, data, ref offset);
        r6.WriteObject(arg6, data, ref offset);
        r7.WriteObject(arg7, data, ref offset);
        r8.WriteObject(arg8, data, ref offset);
        r9.WriteObject(arg9, data, ref offset);
        r10.WriteObject(arg10, data, ref offset);
        r11.WriteObject(arg11, data, ref offset);
        r12.WriteObject(arg12, data, ref offset);
        r13.WriteObject(arg13, data, ref offset);
        r14.WriteObject(arg14, data, ref offset);

        switch (_eventOptions)
        {
          case (EventOptions.OnlyToTarget):
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            break;
          case (EventOptions.OnlyToServer):
            _nhs.SendMessageToServer(_methodId, data);
            break;
          default:
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            else
              _nhs.SendMessageToOthers(_methodId, data);
            break;
        }
      }
    }

    class SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : SyncEventBase
    {
      MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> _handler;
      ReplicationSystem.Replicator<T1> r1;
      ReplicationSystem.Replicator<T2> r2;
      ReplicationSystem.Replicator<T3> r3;
      ReplicationSystem.Replicator<T4> r4;
      ReplicationSystem.Replicator<T5> r5;
      ReplicationSystem.Replicator<T6> r6;
      ReplicationSystem.Replicator<T7> r7;
      ReplicationSystem.Replicator<T8> r8;
      ReplicationSystem.Replicator<T9> r9;
      ReplicationSystem.Replicator<T10> r10;
      ReplicationSystem.Replicator<T11> r11;
      ReplicationSystem.Replicator<T12> r12;
      ReplicationSystem.Replicator<T13> r13;
      ReplicationSystem.Replicator<T14> r14;
      ReplicationSystem.Replicator<T15> r15;

      internal SyncEvent(NetworkHandlerSystem nhs, ReplicationSystem _replicationSystem, ushort methodId,
        EventOptions eventOptions,
        MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> handler) : base(nhs,
        methodId, eventOptions)
      {
        MinMessageSize = 0;
        _handler = handler;

        r1 = _replicationSystem.GetReplicator<T1>();
        MinMessageSize += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(default(T1));

        r2 = _replicationSystem.GetReplicator<T2>();
        MinMessageSize += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(default(T2));

        r3 = _replicationSystem.GetReplicator<T3>();
        MinMessageSize += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(default(T3));

        r4 = _replicationSystem.GetReplicator<T4>();
        MinMessageSize += r4.HaveConstantLength ? r4.ConstantLength : r4.RequestLength(default(T4));

        r5 = _replicationSystem.GetReplicator<T5>();
        MinMessageSize += r5.HaveConstantLength ? r5.ConstantLength : r5.RequestLength(default(T5));

        r6 = _replicationSystem.GetReplicator<T6>();
        MinMessageSize += r6.HaveConstantLength ? r6.ConstantLength : r6.RequestLength(default(T6));

        r7 = _replicationSystem.GetReplicator<T7>();
        MinMessageSize += r7.HaveConstantLength ? r7.ConstantLength : r7.RequestLength(default(T7));

        r8 = _replicationSystem.GetReplicator<T8>();
        MinMessageSize += r8.HaveConstantLength ? r8.ConstantLength : r8.RequestLength(default(T8));

        r9 = _replicationSystem.GetReplicator<T9>();
        MinMessageSize += r9.HaveConstantLength ? r9.ConstantLength : r9.RequestLength(default(T9));

        r10 = _replicationSystem.GetReplicator<T10>();
        MinMessageSize += r10.HaveConstantLength ? r10.ConstantLength : r10.RequestLength(default(T10));

        r11 = _replicationSystem.GetReplicator<T11>();
        MinMessageSize += r11.HaveConstantLength ? r11.ConstantLength : r11.RequestLength(default(T11));

        r12 = _replicationSystem.GetReplicator<T12>();
        MinMessageSize += r12.HaveConstantLength ? r12.ConstantLength : r12.RequestLength(default(T12));

        r13 = _replicationSystem.GetReplicator<T13>();
        MinMessageSize += r13.HaveConstantLength ? r13.ConstantLength : r13.RequestLength(default(T13));

        r14 = _replicationSystem.GetReplicator<T14>();
        MinMessageSize += r14.HaveConstantLength ? r14.ConstantLength : r14.RequestLength(default(T14));

        r15 = _replicationSystem.GetReplicator<T15>();
        MinMessageSize += r15.HaveConstantLength ? r15.ConstantLength : r15.RequestLength(default(T15));
      }

      public override void MessageHandler(byte[] data, int offset, int size, ulong sender)
      {
        _handler(r1.GetObject(data, ref offset), r2.GetObject(data, ref offset), r3.GetObject(data, ref offset),
          r4.GetObject(data, ref offset), r5.GetObject(data, ref offset), r6.GetObject(data, ref offset),
          r7.GetObject(data, ref offset), r8.GetObject(data, ref offset), r9.GetObject(data, ref offset),
          r10.GetObject(data, ref offset), r11.GetObject(data, ref offset), r12.GetObject(data, ref offset),
          r13.GetObject(data, ref offset), r14.GetObject(data, ref offset), r15.GetObject(data, ref offset), sender);
      }

      public void Raise(
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11,
        T12 arg12, T13 arg13, T14 arg14, T15 arg15, ulong target = 0)
      {
        int offset;
        int size = 0;

        size += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(arg1);
        size += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(arg2);
        size += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(arg3);
        size += r4.HaveConstantLength ? r4.ConstantLength : r4.RequestLength(arg4);
        size += r5.HaveConstantLength ? r5.ConstantLength : r5.RequestLength(arg5);
        size += r6.HaveConstantLength ? r6.ConstantLength : r6.RequestLength(arg6);
        size += r7.HaveConstantLength ? r7.ConstantLength : r7.RequestLength(arg7);
        size += r8.HaveConstantLength ? r8.ConstantLength : r8.RequestLength(arg8);
        size += r9.HaveConstantLength ? r9.ConstantLength : r9.RequestLength(arg9);
        size += r10.HaveConstantLength ? r10.ConstantLength : r10.RequestLength(arg10);
        size += r11.HaveConstantLength ? r11.ConstantLength : r11.RequestLength(arg11);
        size += r12.HaveConstantLength ? r12.ConstantLength : r12.RequestLength(arg12);
        size += r13.HaveConstantLength ? r13.ConstantLength : r13.RequestLength(arg13);
        size += r14.HaveConstantLength ? r14.ConstantLength : r14.RequestLength(arg14);
        size += r15.HaveConstantLength ? r15.ConstantLength : r15.RequestLength(arg15);
        var data = _nhs.PrepareMessage(size, out offset);
        r1.WriteObject(arg1, data, ref offset);
        r2.WriteObject(arg2, data, ref offset);
        r3.WriteObject(arg3, data, ref offset);
        r4.WriteObject(arg4, data, ref offset);
        r5.WriteObject(arg5, data, ref offset);
        r6.WriteObject(arg6, data, ref offset);
        r7.WriteObject(arg7, data, ref offset);
        r8.WriteObject(arg8, data, ref offset);
        r9.WriteObject(arg9, data, ref offset);
        r10.WriteObject(arg10, data, ref offset);
        r11.WriteObject(arg11, data, ref offset);
        r12.WriteObject(arg12, data, ref offset);
        r13.WriteObject(arg13, data, ref offset);
        r14.WriteObject(arg14, data, ref offset);
        r15.WriteObject(arg15, data, ref offset);

        switch (_eventOptions)
        {
          case (EventOptions.OnlyToTarget):
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            break;
          case (EventOptions.OnlyToServer):
            _nhs.SendMessageToServer(_methodId, data);
            break;
          default:
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            else
              _nhs.SendMessageToOthers(_methodId, data);
            break;
        }
      }
    }

    class SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : SyncEventBase
    {
      MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> _handler;
      ReplicationSystem.Replicator<T1> r1;
      ReplicationSystem.Replicator<T2> r2;
      ReplicationSystem.Replicator<T3> r3;
      ReplicationSystem.Replicator<T4> r4;
      ReplicationSystem.Replicator<T5> r5;
      ReplicationSystem.Replicator<T6> r6;
      ReplicationSystem.Replicator<T7> r7;
      ReplicationSystem.Replicator<T8> r8;
      ReplicationSystem.Replicator<T9> r9;
      ReplicationSystem.Replicator<T10> r10;
      ReplicationSystem.Replicator<T11> r11;
      ReplicationSystem.Replicator<T12> r12;
      ReplicationSystem.Replicator<T13> r13;
      ReplicationSystem.Replicator<T14> r14;
      ReplicationSystem.Replicator<T15> r15;
      ReplicationSystem.Replicator<T16> r16;

      internal SyncEvent(NetworkHandlerSystem nhs, ReplicationSystem _replicationSystem, ushort methodId,
        EventOptions eventOptions,
        MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> handler) : base(nhs,
        methodId, eventOptions)
      {
        MinMessageSize = 0;
        _handler = handler;

        r1 = _replicationSystem.GetReplicator<T1>();
        MinMessageSize += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(default(T1));

        r2 = _replicationSystem.GetReplicator<T2>();
        MinMessageSize += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(default(T2));

        r3 = _replicationSystem.GetReplicator<T3>();
        MinMessageSize += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(default(T3));

        r4 = _replicationSystem.GetReplicator<T4>();
        MinMessageSize += r4.HaveConstantLength ? r4.ConstantLength : r4.RequestLength(default(T4));

        r5 = _replicationSystem.GetReplicator<T5>();
        MinMessageSize += r5.HaveConstantLength ? r5.ConstantLength : r5.RequestLength(default(T5));

        r6 = _replicationSystem.GetReplicator<T6>();
        MinMessageSize += r6.HaveConstantLength ? r6.ConstantLength : r6.RequestLength(default(T6));

        r7 = _replicationSystem.GetReplicator<T7>();
        MinMessageSize += r7.HaveConstantLength ? r7.ConstantLength : r7.RequestLength(default(T7));

        r8 = _replicationSystem.GetReplicator<T8>();
        MinMessageSize += r8.HaveConstantLength ? r8.ConstantLength : r8.RequestLength(default(T8));

        r9 = _replicationSystem.GetReplicator<T9>();
        MinMessageSize += r9.HaveConstantLength ? r9.ConstantLength : r9.RequestLength(default(T9));

        r10 = _replicationSystem.GetReplicator<T10>();
        MinMessageSize += r10.HaveConstantLength ? r10.ConstantLength : r10.RequestLength(default(T10));

        r11 = _replicationSystem.GetReplicator<T11>();
        MinMessageSize += r11.HaveConstantLength ? r11.ConstantLength : r11.RequestLength(default(T11));

        r12 = _replicationSystem.GetReplicator<T12>();
        MinMessageSize += r12.HaveConstantLength ? r12.ConstantLength : r12.RequestLength(default(T12));

        r13 = _replicationSystem.GetReplicator<T13>();
        MinMessageSize += r13.HaveConstantLength ? r13.ConstantLength : r13.RequestLength(default(T13));

        r14 = _replicationSystem.GetReplicator<T14>();
        MinMessageSize += r14.HaveConstantLength ? r14.ConstantLength : r14.RequestLength(default(T14));

        r15 = _replicationSystem.GetReplicator<T15>();
        MinMessageSize += r15.HaveConstantLength ? r15.ConstantLength : r15.RequestLength(default(T15));

        r16 = _replicationSystem.GetReplicator<T16>();
        MinMessageSize += r16.HaveConstantLength ? r16.ConstantLength : r16.RequestLength(default(T16));
      }

      public override void MessageHandler(byte[] data, int offset, int size, ulong sender)
      {
        _handler(r1.GetObject(data, ref offset), r2.GetObject(data, ref offset), r3.GetObject(data, ref offset),
          r4.GetObject(data, ref offset), r5.GetObject(data, ref offset), r6.GetObject(data, ref offset),
          r7.GetObject(data, ref offset), r8.GetObject(data, ref offset), r9.GetObject(data, ref offset),
          r10.GetObject(data, ref offset), r11.GetObject(data, ref offset), r12.GetObject(data, ref offset),
          r13.GetObject(data, ref offset), r14.GetObject(data, ref offset), r15.GetObject(data, ref offset),
          r16.GetObject(data, ref offset), sender);
      }

      public void Raise(
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11,
        T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, ulong target = 0)
      {
        int offset;
        int size = 0;
        size += r1.HaveConstantLength ? r1.ConstantLength : r1.RequestLength(arg1);
        size += r2.HaveConstantLength ? r2.ConstantLength : r2.RequestLength(arg2);
        size += r3.HaveConstantLength ? r3.ConstantLength : r3.RequestLength(arg3);
        size += r4.HaveConstantLength ? r4.ConstantLength : r4.RequestLength(arg4);
        size += r5.HaveConstantLength ? r5.ConstantLength : r5.RequestLength(arg5);
        size += r6.HaveConstantLength ? r6.ConstantLength : r6.RequestLength(arg6);
        size += r7.HaveConstantLength ? r7.ConstantLength : r7.RequestLength(arg7);
        size += r8.HaveConstantLength ? r8.ConstantLength : r8.RequestLength(arg8);
        size += r9.HaveConstantLength ? r9.ConstantLength : r9.RequestLength(arg9);
        size += r10.HaveConstantLength ? r10.ConstantLength : r10.RequestLength(arg10);
        size += r11.HaveConstantLength ? r11.ConstantLength : r11.RequestLength(arg11);
        size += r12.HaveConstantLength ? r12.ConstantLength : r12.RequestLength(arg12);
        size += r13.HaveConstantLength ? r13.ConstantLength : r13.RequestLength(arg13);
        size += r14.HaveConstantLength ? r14.ConstantLength : r14.RequestLength(arg14);
        size += r15.HaveConstantLength ? r15.ConstantLength : r15.RequestLength(arg15);
        size += r16.HaveConstantLength ? r16.ConstantLength : r16.RequestLength(arg16);
        var data = _nhs.PrepareMessage(size, out offset);
        r1.WriteObject(arg1, data, ref offset);
        r2.WriteObject(arg2, data, ref offset);
        r3.WriteObject(arg3, data, ref offset);
        r4.WriteObject(arg4, data, ref offset);
        r5.WriteObject(arg5, data, ref offset);
        r6.WriteObject(arg6, data, ref offset);
        r7.WriteObject(arg7, data, ref offset);
        r8.WriteObject(arg8, data, ref offset);
        r9.WriteObject(arg9, data, ref offset);
        r10.WriteObject(arg10, data, ref offset);
        r11.WriteObject(arg11, data, ref offset);
        r12.WriteObject(arg12, data, ref offset);
        r13.WriteObject(arg13, data, ref offset);
        r14.WriteObject(arg14, data, ref offset);
        r15.WriteObject(arg15, data, ref offset);
        r16.WriteObject(arg16, data, ref offset);
        switch (_eventOptions)
        {
          case (EventOptions.OnlyToTarget):
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            break;
          case (EventOptions.OnlyToServer):
            _nhs.SendMessageToServer(_methodId, data);
            break;
          default:
            if (target != 0)
              _nhs.SendMessageTo(_methodId, data, target);
            else
              _nhs.SendMessageToOthers(_methodId, data);
            break;
        }
      }
    }

    #endregion

    #region Callers

    public MessageEventCaller Create(MessageEventCaller caller, EventOptions options, bool callAtRaise = false,
      bool registerAlways = false)
    {
      return Create(caller, caller, options, callAtRaise, registerAlways);
    }

    public MessageEventCaller Create(MessageEventCaller clientSide, MessageEventCaller serverSide, EventOptions options,
      bool callAtRaise = false, bool registerAlways = false)
    {
      if (!_initDone)
      {
        MyAPIGateway.Multiplayer.RegisterMessageHandler(_networkId, MPMessageHandler);
        _initDone = true;
      }

      methodId++;
      if (IsMultiplayer || registerAlways)
      {
        if (IsServer && IsClient)
        {
          if (serverSide == null)
          {
            var eve = new SyncEvent(this, methodId, options, clientSide);
            _handlers[methodId] = eve;
            return (target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
                clientSide(target);
              else
                eve.Raise(target);
            };
          }

          if (clientSide == null)
          {
            var eve = new SyncEvent(this, methodId, options, serverSide);
            _handlers[methodId] = eve;
            return eve.Raise;
          }

          var even = new SyncEvent(this, methodId, options, serverSide);
          _handlers[methodId] = even;
          if (callAtRaise)
          {
            return (target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
              {
                serverSide(target);
                clientSide(target);
              }
              else
              {
                serverSide(target);
                even.Raise(target);
              }
            };
          }
        }

        if (IsServer)
        {
          if (serverSide != null)
          {
            var eve = new SyncEvent(this, methodId, options, serverSide);
            _handlers[methodId] = eve;
            if (callAtRaise)
              return (target) =>
              {
                serverSide(target);
                eve.Raise(target);
              };
            return eve.Raise;
          }

          if (clientSide == null)
          {
            return (target) => { };
          }

          return new SyncEvent(this, methodId, options, serverSide).Raise;
        }

        if (clientSide != null)
        {
          var eve = new SyncEvent(this, methodId, options, clientSide);
          _handlers[methodId] = eve;
          if (callAtRaise)
            return (target) =>
            {
              clientSide(target);
              eve.Raise(target);
            };
          return eve.Raise;
        }

        if (serverSide == null)
          return (target) => { };
        return new SyncEvent(this, methodId, options, clientSide).Raise;
      }

      if (serverSide == null)
        return clientSide;
      if (clientSide == null)
        return serverSide;
      return (target) =>
      {
        serverSide(target);
        clientSide(target);
      };
    }

    public MessageEventCaller<T1> Create<T1>(MessageEventCaller<T1> clientSide, MessageEventCaller<T1> serverSide,
      EventOptions options, bool callAtRaise = false, bool registerAlways = false)
    {
      if (!_initDone)
      {
        MyAPIGateway.Multiplayer.RegisterMessageHandler(_networkId, MPMessageHandler);
        _initDone = true;
      }

      methodId++;
      MyLog.Default.WriteLine($"SERVER-IsMultiplayer: {IsMultiplayer}");
      MyLog.Default.Flush();
      if (IsMultiplayer || registerAlways)
      {
        if (IsServer && IsClient)
        {
          if (serverSide == null)
          {
            MyLog.Default.WriteLine($"SERVER-Sync Event");
            MyLog.Default.Flush();
            var eve = new SyncEvent<T1>(this, _replicationSystem, methodId, options, clientSide);
            _handlers[methodId] = eve;
            return (arg1, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
                clientSide(arg1, target);
              else
                eve.Raise(arg1, target);
            };
          }

          if (clientSide == null)
          {
            MyLog.Default.WriteLine($"SERVER-Sync Event");
            MyLog.Default.Flush();
            var eve = new SyncEvent<T1>(this, _replicationSystem, methodId, options, serverSide);
            _handlers[methodId] = eve;
            return serverSide;
          }

          var even = new SyncEvent<T1>(this, _replicationSystem, methodId, options, serverSide);
          _handlers[methodId] = even;
          if (callAtRaise)
          {
            return (arg1, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
              {
                serverSide(arg1, target);
                clientSide(arg1, target);
              }
              else
              {
                serverSide(arg1, target);
                even.Raise(arg1, target);
              }
            };
          }
        }

        if (IsServer)
        {
          if (serverSide != null)
          {
            MyLog.Default.WriteLine($"SERVER-CREATE SyncEvent");
            MyLog.Default.Flush();
            var eve = new SyncEvent<T1>(this, _replicationSystem, methodId, options, serverSide);
            _handlers[methodId] = eve;
            if (callAtRaise)
              return (arg1, target) =>
              {
                serverSide(arg1, target);
                eve.Raise(arg1, target);
              };
            return eve.Raise;
          }

          if (clientSide == null)
          {
            MyLog.Default.WriteLine($"CLIENT NULL SyncEvent");
            MyLog.Default.Flush();
            return (arg1, target) => { };
          }

          return new SyncEvent<T1>(this, _replicationSystem, methodId, options, serverSide).Raise;
        }

        if (clientSide != null)
        {
          MyLog.Default.WriteLine($"Client-Sync");
          MyLog.Default.Flush();
          var eve = new SyncEvent<T1>(this, _replicationSystem, methodId, options, clientSide);
          _handlers[methodId] = eve;
          if (callAtRaise)
            return (arg1, target) =>
            {
              clientSide(arg1, target);
              eve.Raise(arg1, target);
            };
          return eve.Raise;
        }
        MyLog.Default.WriteLine($"Unknown- KMT");
        MyLog.Default.Flush();
        if (serverSide == null)
          return (arg1, target) => { };
        return new SyncEvent<T1>(this, _replicationSystem, methodId, options, clientSide).Raise;
      }

      MyLog.Default.WriteLine($"Unknown 2- KMT");
      MyLog.Default.Flush();
      if (serverSide == null)
        return clientSide;
      if (clientSide == null)
        return serverSide;
      return (arg1, target) =>
      {
        serverSide(arg1, target);
        clientSide(arg1, target);
      };
    }

    public MessageEventCaller<T1, T2> Create<T1, T2>(MessageEventCaller<T1, T2> clientSide,
      MessageEventCaller<T1, T2> serverSide, EventOptions options, bool callAtRaise = false,
      bool registerAlways = false)
    {
      if (!_initDone)
      {
        MyAPIGateway.Multiplayer.RegisterMessageHandler(_networkId, MPMessageHandler);
        _initDone = true;
      }

      methodId++;
      if (IsMultiplayer || registerAlways)
      {
        if (IsServer && IsClient)
        {
          if (serverSide == null)
          {
            var eve = new SyncEvent<T1, T2>(this, _replicationSystem, methodId, options, clientSide);
            _handlers[methodId] = eve;
            return (arg1, arg2, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
                clientSide(arg1, arg2, target);
              else
                eve.Raise(arg1, arg2, target);
            };
          }

          if (clientSide == null)
          {
            var eve = new SyncEvent<T1, T2>(this, _replicationSystem, methodId, options, serverSide);
            _handlers[methodId] = eve;
            return eve.Raise;
          }

          var even = new SyncEvent<T1, T2>(this, _replicationSystem, methodId, options, serverSide);
          _handlers[methodId] = even;
          if (callAtRaise)
          {
            return (arg1, arg2, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
              {
                serverSide(arg1, arg2, target);
                clientSide(arg1, arg2, target);
              }
              else
              {
                serverSide(arg1, arg2, target);
                even.Raise(arg1, arg2, target);
              }
            };
          }
        }

        if (IsServer)
        {
          if (serverSide != null)
          {
            var eve = new SyncEvent<T1, T2>(this, _replicationSystem, methodId, options, serverSide);
            _handlers[methodId] = eve;
            if (callAtRaise)
              return (arg1, arg2, target) =>
              {
                serverSide(arg1, arg2, target);
                eve.Raise(arg1, arg2, target);
              };
            return eve.Raise;
          }

          if (clientSide == null)
          {
            return (arg1, arg2, target) => { };
          }

          return new SyncEvent<T1, T2>(this, _replicationSystem, methodId, options, serverSide).Raise;
        }

        if (clientSide != null)
        {
          var eve = new SyncEvent<T1, T2>(this, _replicationSystem, methodId, options, clientSide);
          _handlers[methodId] = eve;
          if (callAtRaise)
            return (arg1, arg2, target) =>
            {
              clientSide(arg1, arg2, target);
              eve.Raise(arg1, arg2, target);
            };
          return eve.Raise;
        }

        if (serverSide == null)
          return (arg1, arg2, target) => { };
        return new SyncEvent<T1, T2>(this, _replicationSystem, methodId, options, clientSide).Raise;
      }

      if (serverSide == null)
        return clientSide;
      if (clientSide == null)
        return serverSide;
      return (arg1, arg2, target) =>
      {
        serverSide(arg1, arg2, target);
        clientSide(arg1, arg2, target);
      };
    }

    public MessageEventCaller<T1, T2, T3> Create<T1, T2, T3>(MessageEventCaller<T1, T2, T3> clientSide,
      MessageEventCaller<T1, T2, T3> serverSide, EventOptions options, bool callAtRaise = false,
      bool registerAlways = false)
    {
      if (!_initDone)
      {
        MyAPIGateway.Multiplayer.RegisterMessageHandler(_networkId, MPMessageHandler);
        _initDone = true;
      }

      methodId++;
      if (IsMultiplayer || registerAlways)
      {
        if (IsServer && IsClient)
        {
          if (serverSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3>(this, _replicationSystem, methodId, options, clientSide);
            _handlers[methodId] = eve;
            return (arg1, arg2, arg3, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
                clientSide(arg1, arg2, arg3, target);
              else
                eve.Raise(arg1, arg2, arg3, target);
            };
          }

          if (clientSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3>(this, _replicationSystem, methodId, options, serverSide);
            _handlers[methodId] = eve;
            return eve.Raise;
          }

          var even = new SyncEvent<T1, T2, T3>(this, _replicationSystem, methodId, options, serverSide);
          _handlers[methodId] = even;
          if (callAtRaise)
          {
            return (arg1, arg2, arg3, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
              {
                serverSide(arg1, arg2, arg3, target);
                clientSide(arg1, arg2, arg3, target);
              }
              else
              {
                serverSide(arg1, arg2, arg3, target);
                even.Raise(arg1, arg2, arg3, target);
              }
            };
          }
        }

        if (IsServer)
        {
          if (serverSide != null)
          {
            var eve = new SyncEvent<T1, T2, T3>(this, _replicationSystem, methodId, options, serverSide);
            _handlers[methodId] = eve;
            if (callAtRaise)
              return (arg1, arg2, arg3, target) =>
              {
                serverSide(arg1, arg2, arg3, target);
                eve.Raise(arg1, arg2, arg3, target);
              };
            return eve.Raise;
          }

          if (clientSide == null)
          {
            return (arg1, arg2, arg3, target) => { };
          }

          return new SyncEvent<T1, T2, T3>(this, _replicationSystem, methodId, options, serverSide).Raise;
        }

        if (clientSide != null)
        {
          var eve = new SyncEvent<T1, T2, T3>(this, _replicationSystem, methodId, options, clientSide);
          _handlers[methodId] = eve;
          if (callAtRaise)
            return (arg1, arg2, arg3, target) =>
            {
              clientSide(arg1, arg2, arg3, target);
              eve.Raise(arg1, arg2, arg3, target);
            };
          return eve.Raise;
        }

        if (serverSide == null)
          return (arg1, arg2, arg3, target) => { };
        return new SyncEvent<T1, T2, T3>(this, _replicationSystem, methodId, options, clientSide).Raise;
      }

      if (serverSide == null)
        return clientSide;
      if (clientSide == null)
        return serverSide;
      return (arg1, arg2, arg3, target) =>
      {
        serverSide(arg1, arg2, arg3, target);
        clientSide(arg1, arg2, arg3, target);
      };
    }

    public MessageEventCaller<T1, T2, T3, T4> Create<T1, T2, T3, T4>(MessageEventCaller<T1, T2, T3, T4> clientSide,
      MessageEventCaller<T1, T2, T3, T4> serverSide, EventOptions options, bool callAtRaise = false,
      bool registerAlways = false)
    {
      if (!_initDone)
      {
        MyAPIGateway.Multiplayer.RegisterMessageHandler(_networkId, MPMessageHandler);
        _initDone = true;
      }

      methodId++;
      if (IsMultiplayer || registerAlways)
      {
        if (IsServer && IsClient)
        {
          if (serverSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4>(this, _replicationSystem, methodId, options, clientSide);
            _handlers[methodId] = eve;
            return (arg1, arg2, arg3, arg4, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
                clientSide(arg1, arg2, arg3, arg4, target);
              else
                eve.Raise(arg1, arg2, arg3, arg4, target);
            };
          }

          if (clientSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4>(this, _replicationSystem, methodId, options, serverSide);
            _handlers[methodId] = eve;
            return eve.Raise;
          }

          var even = new SyncEvent<T1, T2, T3, T4>(this, _replicationSystem, methodId, options, serverSide);
          _handlers[methodId] = even;
          if (callAtRaise)
          {
            return (arg1, arg2, arg3, arg4, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
              {
                serverSide(arg1, arg2, arg3, arg4, target);
                clientSide(arg1, arg2, arg3, arg4, target);
              }
              else
              {
                serverSide(arg1, arg2, arg3, arg4, target);
                even.Raise(arg1, arg2, arg3, arg4, target);
              }
            };
          }
        }

        if (IsServer)
        {
          if (serverSide != null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4>(this, _replicationSystem, methodId, options, serverSide);
            _handlers[methodId] = eve;
            if (callAtRaise)
              return (arg1, arg2, arg3, arg4, target) =>
              {
                serverSide(arg1, arg2, arg3, arg4, target);
                eve.Raise(arg1, arg2, arg3, arg4, target);
              };
            return eve.Raise;
          }

          if (clientSide == null)
          {
            return (arg1, arg2, arg3, arg4, target) => { };
          }

          return new SyncEvent<T1, T2, T3, T4>(this, _replicationSystem, methodId, options, serverSide).Raise;
        }

        if (clientSide != null)
        {
          var eve = new SyncEvent<T1, T2, T3, T4>(this, _replicationSystem, methodId, options, clientSide);
          _handlers[methodId] = eve;
          if (callAtRaise)
            return (arg1, arg2, arg3, arg4, target) =>
            {
              clientSide(arg1, arg2, arg3, arg4, target);
              eve.Raise(arg1, arg2, arg3, arg4, target);
            };
          return eve.Raise;
        }

        if (serverSide == null)
          return (arg1, arg2, arg3, arg4, target) => { };
        return new SyncEvent<T1, T2, T3, T4>(this, _replicationSystem, methodId, options, clientSide).Raise;
      }

      if (serverSide == null)
        return clientSide;
      if (clientSide == null)
        return serverSide;
      return (arg1, arg2, arg3, arg4, target) =>
      {
        serverSide(arg1, arg2, arg3, arg4, target);
        clientSide(arg1, arg2, arg3, arg4, target);
      };
    }

    public MessageEventCaller<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(
      MessageEventCaller<T1, T2, T3, T4, T5> clientSide, MessageEventCaller<T1, T2, T3, T4, T5> serverSide,
      EventOptions options, bool callAtRaise = false, bool registerAlways = false)
    {
      if (!_initDone)
      {
        MyAPIGateway.Multiplayer.RegisterMessageHandler(_networkId, MPMessageHandler);
        _initDone = true;
      }

      methodId++;
      if (IsMultiplayer || registerAlways)
      {
        if (IsServer && IsClient)
        {
          if (serverSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5>(this, _replicationSystem, methodId, options, clientSide);
            _handlers[methodId] = eve;
            return (arg1, arg2, arg3, arg4, arg5, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
                clientSide(arg1, arg2, arg3, arg4, arg5, target);
              else
                eve.Raise(arg1, arg2, arg3, arg4, arg5, target);
            };
          }

          if (clientSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5>(this, _replicationSystem, methodId, options, serverSide);
            _handlers[methodId] = eve;
            return eve.Raise;
          }

          var even = new SyncEvent<T1, T2, T3, T4, T5>(this, _replicationSystem, methodId, options, serverSide);
          _handlers[methodId] = even;
          if (callAtRaise)
          {
            return (arg1, arg2, arg3, arg4, arg5, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, target);
                clientSide(arg1, arg2, arg3, arg4, arg5, target);
              }
              else
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, target);
                even.Raise(arg1, arg2, arg3, arg4, arg5, target);
              }
            };
          }
        }

        if (IsServer)
        {
          if (serverSide != null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5>(this, _replicationSystem, methodId, options, serverSide);
            _handlers[methodId] = eve;
            if (callAtRaise)
              return (arg1, arg2, arg3, arg4, arg5, target) =>
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, target);
                eve.Raise(arg1, arg2, arg3, arg4, arg5, target);
              };
            return eve.Raise;
          }

          if (clientSide == null)
          {
            return (arg1, arg2, arg3, arg4, arg5, target) => { };
          }

          return new SyncEvent<T1, T2, T3, T4, T5>(this, _replicationSystem, methodId, options, serverSide).Raise;
        }

        if (clientSide != null)
        {
          var eve = new SyncEvent<T1, T2, T3, T4, T5>(this, _replicationSystem, methodId, options, clientSide);
          _handlers[methodId] = eve;
          if (callAtRaise)
            return (arg1, arg2, arg3, arg4, arg5, target) =>
            {
              clientSide(arg1, arg2, arg3, arg4, arg5, target);
              eve.Raise(arg1, arg2, arg3, arg4, arg5, target);
            };
          return eve.Raise;
        }

        if (serverSide == null)
          return (arg1, arg2, arg3, arg4, arg5, target) => { };
        return new SyncEvent<T1, T2, T3, T4, T5>(this, _replicationSystem, methodId, options, clientSide).Raise;
      }

      if (serverSide == null)
        return clientSide;
      if (clientSide == null)
        return serverSide;
      return (arg1, arg2, arg3, arg4, arg5, target) =>
      {
        serverSide(arg1, arg2, arg3, arg4, arg5, target);
        clientSide(arg1, arg2, arg3, arg4, arg5, target);
      };
    }

    public MessageEventCaller<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(
      MessageEventCaller<T1, T2, T3, T4, T5, T6> clientSide, MessageEventCaller<T1, T2, T3, T4, T5, T6> serverSide,
      EventOptions options, bool callAtRaise = false, bool registerAlways = false)
    {
      if (!_initDone)
      {
        MyAPIGateway.Multiplayer.RegisterMessageHandler(_networkId, MPMessageHandler);
        _initDone = true;
      }

      methodId++;
      if (IsMultiplayer || registerAlways)
      {
        if (IsServer && IsClient)
        {
          if (serverSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6>(this, _replicationSystem, methodId, options, clientSide);
            _handlers[methodId] = eve;
            return (arg1, arg2, arg3, arg4, arg5, arg6, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
                clientSide(arg1, arg2, arg3, arg4, arg5, arg6, target);
              else
                eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, target);
            };
          }

          if (clientSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6>(this, _replicationSystem, methodId, options, serverSide);
            _handlers[methodId] = eve;
            return eve.Raise;
          }

          var even = new SyncEvent<T1, T2, T3, T4, T5, T6>(this, _replicationSystem, methodId, options, serverSide);
          _handlers[methodId] = even;
          if (callAtRaise)
          {
            return (arg1, arg2, arg3, arg4, arg5, arg6, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, target);
                clientSide(arg1, arg2, arg3, arg4, arg5, arg6, target);
              }
              else
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, target);
                even.Raise(arg1, arg2, arg3, arg4, arg5, arg6, target);
              }
            };
          }
        }

        if (IsServer)
        {
          if (serverSide != null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6>(this, _replicationSystem, methodId, options, serverSide);
            _handlers[methodId] = eve;
            if (callAtRaise)
              return (arg1, arg2, arg3, arg4, arg5, arg6, target) =>
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, target);
                eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, target);
              };
            return eve.Raise;
          }

          if (clientSide == null)
          {
            return (arg1, arg2, arg3, arg4, arg5, arg6, target) => { };
          }

          return new SyncEvent<T1, T2, T3, T4, T5, T6>(this, _replicationSystem, methodId, options, serverSide).Raise;
        }

        if (clientSide != null)
        {
          var eve = new SyncEvent<T1, T2, T3, T4, T5, T6>(this, _replicationSystem, methodId, options, clientSide);
          _handlers[methodId] = eve;
          if (callAtRaise)
            return (arg1, arg2, arg3, arg4, arg5, arg6, target) =>
            {
              clientSide(arg1, arg2, arg3, arg4, arg5, arg6, target);
              eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, target);
            };
          return eve.Raise;
        }

        if (serverSide == null)
          return (arg1, arg2, arg3, arg4, arg5, arg6, target) => { };
        return new SyncEvent<T1, T2, T3, T4, T5, T6>(this, _replicationSystem, methodId, options, clientSide).Raise;
      }

      if (serverSide == null)
        return clientSide;
      if (clientSide == null)
        return serverSide;
      return (arg1, arg2, arg3, arg4, arg5, arg6, target) =>
      {
        serverSide(arg1, arg2, arg3, arg4, arg5, arg6, target);
        clientSide(arg1, arg2, arg3, arg4, arg5, arg6, target);
      };
    }

    public MessageEventCaller<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>(
      MessageEventCaller<T1, T2, T3, T4, T5, T6, T7> clientSide,
      MessageEventCaller<T1, T2, T3, T4, T5, T6, T7> serverSide, EventOptions options, bool callAtRaise = false,
      bool registerAlways = false)
    {
      if (!_initDone)
      {
        MyAPIGateway.Multiplayer.RegisterMessageHandler(_networkId, MPMessageHandler);
        _initDone = true;
      }

      methodId++;
      if (IsMultiplayer || registerAlways)
      {
        if (IsServer && IsClient)
        {
          if (serverSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7>(this, _replicationSystem, methodId, options,
              clientSide);
            _handlers[methodId] = eve;
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
                clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, target);
              else
                eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, target);
            };
          }

          if (clientSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7>(this, _replicationSystem, methodId, options,
              serverSide);
            _handlers[methodId] = eve;
            return eve.Raise;
          }

          var even = new SyncEvent<T1, T2, T3, T4, T5, T6, T7>(this, _replicationSystem, methodId, options, serverSide);
          _handlers[methodId] = even;
          if (callAtRaise)
          {
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, target);
                clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, target);
              }
              else
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, target);
                even.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, target);
              }
            };
          }
        }

        if (IsServer)
        {
          if (serverSide != null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7>(this, _replicationSystem, methodId, options,
              serverSide);
            _handlers[methodId] = eve;
            if (callAtRaise)
              return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, target) =>
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, target);
                eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, target);
              };
            return eve.Raise;
          }

          if (clientSide == null)
          {
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, target) => { };
          }

          return new SyncEvent<T1, T2, T3, T4, T5, T6, T7>(this, _replicationSystem, methodId, options, serverSide)
            .Raise;
        }

        if (clientSide != null)
        {
          var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7>(this, _replicationSystem, methodId, options, clientSide);
          _handlers[methodId] = eve;
          if (callAtRaise)
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, target) =>
            {
              clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, target);
              eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, target);
            };
          return eve.Raise;
        }

        if (serverSide == null)
          return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, target) => { };
        return new SyncEvent<T1, T2, T3, T4, T5, T6, T7>(this, _replicationSystem, methodId, options, clientSide).Raise;
      }

      if (serverSide == null)
        return clientSide;
      if (clientSide == null)
        return serverSide;
      return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, target) =>
      {
        serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, target);
        clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, target);
      };
    }

    public MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8> Create<T1, T2, T3, T4, T5, T6, T7, T8>(
      MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8> clientSide,
      MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8> serverSide, EventOptions options, bool callAtRaise = false,
      bool registerAlways = false)
    {
      if (!_initDone)
      {
        MyAPIGateway.Multiplayer.RegisterMessageHandler(_networkId, MPMessageHandler);
        _initDone = true;
      }

      methodId++;
      if (IsMultiplayer || registerAlways)
      {
        if (IsServer && IsClient)
        {
          if (serverSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8>(this, _replicationSystem, methodId, options,
              clientSide);
            _handlers[methodId] = eve;
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
                clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, target);
              else
                eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, target);
            };
          }

          if (clientSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8>(this, _replicationSystem, methodId, options,
              serverSide);
            _handlers[methodId] = eve;
            return eve.Raise;
          }

          var even = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8>(this, _replicationSystem, methodId, options,
            serverSide);
          _handlers[methodId] = even;
          if (callAtRaise)
          {
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, target);
                clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, target);
              }
              else
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, target);
                even.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, target);
              }
            };
          }
        }

        if (IsServer)
        {
          if (serverSide != null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8>(this, _replicationSystem, methodId, options,
              serverSide);
            _handlers[methodId] = eve;
            if (callAtRaise)
              return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, target) =>
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, target);
                eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, target);
              };
            return eve.Raise;
          }

          if (clientSide == null)
          {
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, target) => { };
          }

          return new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8>(this, _replicationSystem, methodId, options, serverSide)
            .Raise;
        }

        if (clientSide != null)
        {
          var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8>(this, _replicationSystem, methodId, options,
            clientSide);
          _handlers[methodId] = eve;
          if (callAtRaise)
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, target) =>
            {
              clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, target);
              eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, target);
            };
          return eve.Raise;
        }

        if (serverSide == null)
          return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, target) => { };
        return new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8>(this, _replicationSystem, methodId, options, clientSide)
          .Raise;
      }

      if (serverSide == null)
        return clientSide;
      if (clientSide == null)
        return serverSide;
      return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, target) =>
      {
        serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, target);
        clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, target);
      };
    }

    public MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
      MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9> clientSide,
      MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9> serverSide, EventOptions options, bool callAtRaise = false,
      bool registerAlways = false)
    {
      if (!_initDone)
      {
        MyAPIGateway.Multiplayer.RegisterMessageHandler(_networkId, MPMessageHandler);
        _initDone = true;
      }

      methodId++;
      if (IsMultiplayer || registerAlways)
      {
        if (IsServer && IsClient)
        {
          if (serverSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this, _replicationSystem, methodId, options,
              clientSide);
            _handlers[methodId] = eve;
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
                clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, target);
              else
                eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, target);
            };
          }

          if (clientSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this, _replicationSystem, methodId, options,
              serverSide);
            _handlers[methodId] = eve;
            return eve.Raise;
          }

          var even = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this, _replicationSystem, methodId, options,
            serverSide);
          _handlers[methodId] = even;
          if (callAtRaise)
          {
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, target);
                clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, target);
              }
              else
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, target);
                even.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, target);
              }
            };
          }
        }

        if (IsServer)
        {
          if (serverSide != null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this, _replicationSystem, methodId, options,
              serverSide);
            _handlers[methodId] = eve;
            if (callAtRaise)
              return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, target) =>
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, target);
                eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, target);
              };
            return eve.Raise;
          }

          if (clientSide == null)
          {
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, target) => { };
          }

          return new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this, _replicationSystem, methodId, options,
            serverSide).Raise;
        }

        if (clientSide != null)
        {
          var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this, _replicationSystem, methodId, options,
            clientSide);
          _handlers[methodId] = eve;
          if (callAtRaise)
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, target) =>
            {
              clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, target);
              eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, target);
            };
          return eve.Raise;
        }

        if (serverSide == null)
          return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, target) => { };
        return new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this, _replicationSystem, methodId, options,
          clientSide).Raise;
      }

      if (serverSide == null)
        return clientSide;
      if (clientSide == null)
        return serverSide;
      return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, target) =>
      {
        serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, target);
        clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, target);
      };
    }

    public MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
      MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> clientSide,
      MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> serverSide, EventOptions options,
      bool callAtRaise = false, bool registerAlways = false)
    {
      if (!_initDone)
      {
        MyAPIGateway.Multiplayer.RegisterMessageHandler(_networkId, MPMessageHandler);
        _initDone = true;
      }

      methodId++;
      if (IsMultiplayer || registerAlways)
      {
        if (IsServer && IsClient)
        {
          if (serverSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this, _replicationSystem, methodId,
              options, clientSide);
            _handlers[methodId] = eve;
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
                clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, target);
              else
                eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, target);
            };
          }

          if (clientSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this, _replicationSystem, methodId,
              options, serverSide);
            _handlers[methodId] = eve;
            return eve.Raise;
          }

          var even = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this, _replicationSystem, methodId, options,
            serverSide);
          _handlers[methodId] = even;
          if (callAtRaise)
          {
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, target);
                clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, target);
              }
              else
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, target);
                even.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, target);
              }
            };
          }
        }

        if (IsServer)
        {
          if (serverSide != null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this, _replicationSystem, methodId,
              options, serverSide);
            _handlers[methodId] = eve;
            if (callAtRaise)
              return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, target) =>
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, target);
                eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, target);
              };
            return eve.Raise;
          }

          if (clientSide == null)
          {
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, target) => { };
          }

          return new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this, _replicationSystem, methodId, options,
            serverSide).Raise;
        }

        if (clientSide != null)
        {
          var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this, _replicationSystem, methodId, options,
            clientSide);
          _handlers[methodId] = eve;
          if (callAtRaise)
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, target) =>
            {
              clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, target);
              eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, target);
            };
          return eve.Raise;
        }

        if (serverSide == null)
          return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, target) => { };
        return new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this, _replicationSystem, methodId, options,
          clientSide).Raise;
      }

      if (serverSide == null)
        return clientSide;
      if (clientSide == null)
        return serverSide;
      return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, target) =>
      {
        serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, target);
        clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, target);
      };
    }

    public MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
      Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> clientSide,
        MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> serverSide, EventOptions options,
        bool callAtRaise = false, bool registerAlways = false)
    {
      if (!_initDone)
      {
        MyAPIGateway.Multiplayer.RegisterMessageHandler(_networkId, MPMessageHandler);
        _initDone = true;
      }

      methodId++;
      if (IsMultiplayer || registerAlways)
      {
        if (IsServer && IsClient)
        {
          if (serverSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this, _replicationSystem, methodId,
              options, clientSide);
            _handlers[methodId] = eve;
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
                clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, target);
              else
                eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, target);
            };
          }

          if (clientSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this, _replicationSystem, methodId,
              options, serverSide);
            _handlers[methodId] = eve;
            return eve.Raise;
          }

          var even = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this, _replicationSystem, methodId,
            options, serverSide);
          _handlers[methodId] = even;
          if (callAtRaise)
          {
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, target);
                clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, target);
              }
              else
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, target);
                even.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, target);
              }
            };
          }
        }

        if (IsServer)
        {
          if (serverSide != null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this, _replicationSystem, methodId,
              options, serverSide);
            _handlers[methodId] = eve;
            if (callAtRaise)
              return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, target) =>
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, target);
                eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, target);
              };
            return eve.Raise;
          }

          if (clientSide == null)
          {
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, target) => { };
          }

          return new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this, _replicationSystem, methodId,
            options, serverSide).Raise;
        }

        if (clientSide != null)
        {
          var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this, _replicationSystem, methodId,
            options, clientSide);
          _handlers[methodId] = eve;
          if (callAtRaise)
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, target) =>
            {
              clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, target);
              eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, target);
            };
          return eve.Raise;
        }

        if (serverSide == null)
          return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, target) => { };
        return new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this, _replicationSystem, methodId, options,
          clientSide).Raise;
      }

      if (serverSide == null)
        return clientSide;
      if (clientSide == null)
        return serverSide;
      return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, target) =>
      {
        serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, target);
        clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, target);
      };
    }

    public MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
      Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> clientSide,
        MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> serverSide, EventOptions options,
        bool callAtRaise = false, bool registerAlways = false)
    {
      if (!_initDone)
      {
        MyAPIGateway.Multiplayer.RegisterMessageHandler(_networkId, MPMessageHandler);
        _initDone = true;
      }

      methodId++;
      if (IsMultiplayer || registerAlways)
      {
        if (IsServer && IsClient)
        {
          if (serverSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this, _replicationSystem,
              methodId, options, clientSide);
            _handlers[methodId] = eve;
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
                clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, target);
              else
                eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, target);
            };
          }

          if (clientSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this, _replicationSystem,
              methodId, options, serverSide);
            _handlers[methodId] = eve;
            return eve.Raise;
          }

          var even = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this, _replicationSystem,
            methodId, options, serverSide);
          _handlers[methodId] = even;
          if (callAtRaise)
          {
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, target);
                clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, target);
              }
              else
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, target);
                even.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, target);
              }
            };
          }
        }

        if (IsServer)
        {
          if (serverSide != null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this, _replicationSystem,
              methodId, options, serverSide);
            _handlers[methodId] = eve;
            if (callAtRaise)
              return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, target) =>
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, target);
                eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, target);
              };
            return eve.Raise;
          }

          if (clientSide == null)
          {
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, target) => { };
          }

          return new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this, _replicationSystem, methodId,
            options, serverSide).Raise;
        }

        if (clientSide != null)
        {
          var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this, _replicationSystem, methodId,
            options, clientSide);
          _handlers[methodId] = eve;
          if (callAtRaise)
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, target) =>
            {
              clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, target);
              eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, target);
            };
          return eve.Raise;
        }

        if (serverSide == null)
          return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, target) => { };
        return new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this, _replicationSystem, methodId,
          options, clientSide).Raise;
      }

      if (serverSide == null)
        return clientSide;
      if (clientSide == null)
        return serverSide;
      return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, target) =>
      {
        serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, target);
        clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, target);
      };
    }

    public MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
      Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> clientSide,
        MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> serverSide, EventOptions options,
        bool callAtRaise = false, bool registerAlways = false)
    {
      if (!_initDone)
      {
        MyAPIGateway.Multiplayer.RegisterMessageHandler(_networkId, MPMessageHandler);
        _initDone = true;
      }

      methodId++;
      if (IsMultiplayer || registerAlways)
      {
        if (IsServer && IsClient)
        {
          if (serverSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this, _replicationSystem,
              methodId, options, clientSide);
            _handlers[methodId] = eve;
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
                clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, target);
              else
                eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, target);
            };
          }

          if (clientSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this, _replicationSystem,
              methodId, options, serverSide);
            _handlers[methodId] = eve;
            return eve.Raise;
          }

          var even = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this, _replicationSystem,
            methodId, options, serverSide);
          _handlers[methodId] = even;
          if (callAtRaise)
          {
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, target);
                clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, target);
              }
              else
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, target);
                even.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, target);
              }
            };
          }
        }

        if (IsServer)
        {
          if (serverSide != null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this, _replicationSystem,
              methodId, options, serverSide);
            _handlers[methodId] = eve;
            if (callAtRaise)
              return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, target) =>
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, target);
                eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, target);
              };
            return eve.Raise;
          }

          if (clientSide == null)
          {
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, target) => { };
          }

          return new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this, _replicationSystem,
            methodId, options, serverSide).Raise;
        }

        if (clientSide != null)
        {
          var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this, _replicationSystem,
            methodId, options, clientSide);
          _handlers[methodId] = eve;
          if (callAtRaise)
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, target) =>
            {
              clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, target);
              eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, target);
            };
          return eve.Raise;
        }

        if (serverSide == null)
          return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, target) => { };
        return new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this, _replicationSystem, methodId,
          options, clientSide).Raise;
      }

      if (serverSide == null)
        return clientSide;
      if (clientSide == null)
        return serverSide;
      return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, target) =>
      {
        serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, target);
        clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, target);
      };
    }

    public MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
      Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> clientSide,
        MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> serverSide,
        EventOptions options, bool callAtRaise = false, bool registerAlways = false)
    {
      if (!_initDone)
      {
        MyAPIGateway.Multiplayer.RegisterMessageHandler(_networkId, MPMessageHandler);
        _initDone = true;
      }

      methodId++;
      if (IsMultiplayer || registerAlways)
      {
        if (IsServer && IsClient)
        {
          if (serverSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this,
              _replicationSystem, methodId, options, clientSide);
            _handlers[methodId] = eve;
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
                clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                  target);
              else
                eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                  target);
            };
          }

          if (clientSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this,
              _replicationSystem, methodId, options, serverSide);
            _handlers[methodId] = eve;
            return eve.Raise;
          }

          var even = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this,
            _replicationSystem, methodId, options, serverSide);
          _handlers[methodId] = even;
          if (callAtRaise)
          {
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                  target);
                clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                  target);
              }
              else
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                  target);
                even.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                  target);
              }
            };
          }
        }

        if (IsServer)
        {
          if (serverSide != null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this,
              _replicationSystem, methodId, options, serverSide);
            _handlers[methodId] = eve;
            if (callAtRaise)
              return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                target) =>
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                  target);
                eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                  target);
              };
            return eve.Raise;
          }

          if (clientSide == null)
          {
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, target) =>
            {
            };
          }

          return new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this, _replicationSystem,
            methodId, options, serverSide).Raise;
        }

        if (clientSide != null)
        {
          var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this, _replicationSystem,
            methodId, options, clientSide);
          _handlers[methodId] = eve;
          if (callAtRaise)
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, target) =>
            {
              clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                target);
              eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                target);
            };
          return eve.Raise;
        }

        if (serverSide == null)
          return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, target) =>
          {
          };
        return new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this, _replicationSystem,
          methodId, options, clientSide).Raise;
      }

      if (serverSide == null)
        return clientSide;
      if (clientSide == null)
        return serverSide;
      return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, target) =>
      {
        serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, target);
        clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, target);
      };
    }

    public MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
      Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
        MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> clientSide,
        MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> serverSide,
        EventOptions options, bool callAtRaise = false, bool registerAlways = false)
    {
      if (!_initDone)
      {
        MyAPIGateway.Multiplayer.RegisterMessageHandler(_networkId, MPMessageHandler);
        _initDone = true;
      }

      methodId++;
      if (IsMultiplayer || registerAlways)
      {
        if (IsServer && IsClient)
        {
          if (serverSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this,
              _replicationSystem, methodId, options, clientSide);
            _handlers[methodId] = eve;
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15,
              target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
                clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                  arg15, target);
              else
                eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                  arg15, target);
            };
          }

          if (clientSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this,
              _replicationSystem, methodId, options, serverSide);
            _handlers[methodId] = eve;
            return eve.Raise;
          }

          var even = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this,
            _replicationSystem, methodId, options, serverSide);
          _handlers[methodId] = even;
          if (callAtRaise)
          {
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15,
              target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                  arg15, target);
                clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                  arg15, target);
              }
              else
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                  arg15, target);
                even.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                  arg15, target);
              }
            };
          }
        }

        if (IsServer)
        {
          if (serverSide != null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this,
              _replicationSystem, methodId, options, serverSide);
            _handlers[methodId] = eve;
            if (callAtRaise)
              return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15,
                target) =>
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                  arg15, target);
                eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                  arg15, target);
              };
            return eve.Raise;
          }

          if (clientSide == null)
          {
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15,
              target) =>
            {
            };
          }

          return new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this,
            _replicationSystem, methodId, options, serverSide).Raise;
        }

        if (clientSide != null)
        {
          var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this,
            _replicationSystem, methodId, options, clientSide);
          _handlers[methodId] = eve;
          if (callAtRaise)
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15,
              target) =>
            {
              clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15,
                target);
              eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15,
                target);
            };
          return eve.Raise;
        }

        if (serverSide == null)
          return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15,
            target) =>
          {
          };
        return new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this, _replicationSystem,
          methodId, options, clientSide).Raise;
      }

      if (serverSide == null)
        return clientSide;
      if (clientSide == null)
        return serverSide;
      return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, target) =>
      {
        serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15,
          target);
        clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15,
          target);
      };
    }

    public MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
      Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
        MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> clientSide,
        MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> serverSide,
        EventOptions options, bool callAtRaise = false, bool registerAlways = false)
    {
      if (!_initDone)
      {
        MyAPIGateway.Multiplayer.RegisterMessageHandler(_networkId, MPMessageHandler);
        _initDone = true;
      }

      methodId++;
      if (IsMultiplayer || registerAlways)
      {
        if (IsServer && IsClient)
        {
          if (serverSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this,
              _replicationSystem, methodId, options, clientSide);
            _handlers[methodId] = eve;
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15,
              arg16, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
                clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                  arg15, arg16, target);
              else
                eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                  arg15, arg16, target);
            };
          }

          if (clientSide == null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this,
              _replicationSystem, methodId, options, serverSide);
            _handlers[methodId] = eve;
            return eve.Raise;
          }

          var even = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this,
            _replicationSystem, methodId, options, serverSide);
          _handlers[methodId] = even;
          if (callAtRaise)
          {
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15,
              arg16, target) =>
            {
              if (target == MyAPIGateway.Session.Player.SteamUserId)
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                  arg15, arg16, target);
                clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                  arg15, arg16, target);
              }
              else
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                  arg15, arg16, target);
                even.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                  arg15, arg16, target);
              }
            };
          }
        }

        if (IsServer)
        {
          if (serverSide != null)
          {
            var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this,
              _replicationSystem, methodId, options, serverSide);
            _handlers[methodId] = eve;
            if (callAtRaise)
              return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15,
                arg16, target) =>
              {
                serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                  arg15, arg16, target);
                eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                  arg15, arg16, target);
              };
            return eve.Raise;
          }

          if (clientSide == null)
          {
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15,
              arg16, target) =>
            {
            };
          }

          return new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this,
            _replicationSystem, methodId, options, serverSide).Raise;
        }

        if (clientSide != null)
        {
          var eve = new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this,
            _replicationSystem, methodId, options, clientSide);
          _handlers[methodId] = eve;
          if (callAtRaise)
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15,
              arg16, target) =>
            {
              clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15,
                arg16, target);
              eve.Raise(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15,
                arg16, target);
            };
          return eve.Raise;
        }

        if (serverSide == null)
          return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16,
            target) =>
          {
          };
        return new SyncEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this,
          _replicationSystem, methodId, options, clientSide).Raise;
      }

      if (serverSide == null)
        return clientSide;
      if (clientSide == null)
        return serverSide;
      return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16,
        target) =>
      {
        serverSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15,
          arg16, target);
        clientSide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15,
          arg16, target);
      };
    }

    public MessageEventCaller<T1> Create<T1>(MessageEventCaller<T1> caller, EventOptions options,
      bool callAtRaise = false, bool registerAlways = false)
    {
      return Create(caller, caller, options, callAtRaise, registerAlways);
    }

    public MessageEventCaller<T1, T2> Create<T1, T2>(MessageEventCaller<T1, T2> caller, EventOptions options,
      bool callAtRaise = false, bool registerAlways = false)
    {
      return Create(caller, caller, options, callAtRaise, registerAlways);
    }

    public MessageEventCaller<T1, T2, T3> Create<T1, T2, T3>(MessageEventCaller<T1, T2, T3> caller,
      EventOptions options, bool callAtRaise = false, bool registerAlways = false)
    {
      return Create(caller, caller, options, callAtRaise, registerAlways);
    }

    public MessageEventCaller<T1, T2, T3, T4> Create<T1, T2, T3, T4>(MessageEventCaller<T1, T2, T3, T4> caller,
      EventOptions options, bool callAtRaise = false, bool registerAlways = false)
    {
      return Create(caller, caller, options, callAtRaise, registerAlways);
    }

    public MessageEventCaller<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(
      MessageEventCaller<T1, T2, T3, T4, T5> caller, EventOptions options, bool callAtRaise = false,
      bool registerAlways = false)
    {
      return Create(caller, caller, options, callAtRaise, registerAlways);
    }

    public MessageEventCaller<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(
      MessageEventCaller<T1, T2, T3, T4, T5, T6> caller, EventOptions options, bool callAtRaise = false,
      bool registerAlways = false)
    {
      return Create(caller, caller, options, callAtRaise, registerAlways);
    }

    public MessageEventCaller<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>(
      MessageEventCaller<T1, T2, T3, T4, T5, T6, T7> caller, EventOptions options, bool callAtRaise = false,
      bool registerAlways = false)
    {
      return Create(caller, caller, options, callAtRaise, registerAlways);
    }

    public MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8> Create<T1, T2, T3, T4, T5, T6, T7, T8>(
      MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8> caller, EventOptions options, bool callAtRaise = false,
      bool registerAlways = false)
    {
      return Create(caller, caller, options, callAtRaise, registerAlways);
    }

    public MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
      MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9> caller, EventOptions options, bool callAtRaise = false,
      bool registerAlways = false)
    {
      return Create(caller, caller, options, callAtRaise, registerAlways);
    }

    public MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
      MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> caller, EventOptions options,
      bool callAtRaise = false, bool registerAlways = false)
    {
      return Create(caller, caller, options, callAtRaise, registerAlways);
    }

    public MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
      Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> caller, EventOptions options,
        bool callAtRaise = false, bool registerAlways = false)
    {
      return Create(caller, caller, options, callAtRaise, registerAlways);
    }

    public MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
      Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> caller, EventOptions options,
        bool callAtRaise = false, bool registerAlways = false)
    {
      return Create(caller, caller, options, callAtRaise, registerAlways);
    }

    public MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
      Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> caller, EventOptions options,
        bool callAtRaise = false, bool registerAlways = false)
    {
      return Create(caller, caller, options, callAtRaise, registerAlways);
    }

    public MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
      Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> caller, EventOptions options,
        bool callAtRaise = false, bool registerAlways = false)
    {
      return Create(caller, caller, options, callAtRaise, registerAlways);
    }

    public MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Create<T1, T2, T3, T4,
      T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
      MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> caller, EventOptions options,
      bool callAtRaise = false, bool registerAlways = false)
    {
      return Create(caller, caller, options, callAtRaise, registerAlways);
    }

    public MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Create<T1, T2, T3,
      T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
      MessageEventCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> caller,
      EventOptions options, bool callAtRaise = false, bool registerAlways = false)
    {
      return Create(caller, caller, options, callAtRaise, registerAlways);
    }

    #endregion
  }
}