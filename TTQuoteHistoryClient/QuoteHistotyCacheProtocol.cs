namespace SoftFX.Net.QuoteHistoryCacheProtocol
{
    using System;
    using System.IO;
    using SoftFX.Net.Core;
    
    enum PriceType
    {
        Bid = 0,
        Ask = 1,
    }
    
    struct PriceTypeArray
    {
        public PriceTypeArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public PriceType this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                data_.SetUInt(itemOffset, (uint) value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                return (PriceType) data_.GetUInt(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PriceTypeNullArray
    {
        public PriceTypeNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public PriceType? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                if (value.HasValue)
                {
                    data_.SetUIntNull(itemOffset, (uint) value.Value);
                }
                else
                    data_.SetUIntNull(itemOffset, null);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                uint? value = data_.GetUIntNull(itemOffset);
                
                if (value.HasValue)
                    return (PriceType) value.Value;
                
                return null;
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct Bar
    {
        public Bar(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public DateTime Time
        {
            set { data_.SetDateTime(offset_ + 0, value); }
            
            get { return data_.GetDateTime(offset_ + 0); }
        }
        
        public double Open
        {
            set { data_.SetDouble(offset_ + 8, value); }
            
            get { return data_.GetDouble(offset_ + 8); }
        }
        
        public double High
        {
            set { data_.SetDouble(offset_ + 16, value); }
            
            get { return data_.GetDouble(offset_ + 16); }
        }
        
        public double Low
        {
            set { data_.SetDouble(offset_ + 24, value); }
            
            get { return data_.GetDouble(offset_ + 24); }
        }
        
        public double Close
        {
            set { data_.SetDouble(offset_ + 32, value); }
            
            get { return data_.GetDouble(offset_ + 32); }
        }
        
        public double Volume
        {
            set { data_.SetDouble(offset_ + 40, value); }
            
            get { return data_.GetDouble(offset_ + 40); }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct BarArray
    {
        public BarArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 48);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public Bar this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 48);
                
                return new Bar(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct TickId
    {
        public TickId(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public DateTime Time
        {
            set { data_.SetDateTime(offset_ + 0, value); }
            
            get { return data_.GetDateTime(offset_ + 0); }
        }
        
        public byte Index
        {
            set { data_.SetByte(offset_ + 8, value); }
            
            get { return data_.GetByte(offset_ + 8); }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct TickIdArray
    {
        public TickIdArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 9);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public TickId this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 9);
                
                return new TickId(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct Level2Value
    {
        public Level2Value(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public double Price
        {
            set { data_.SetDouble(offset_ + 0, value); }
            
            get { return data_.GetDouble(offset_ + 0); }
        }
        
        public double Volume
        {
            set { data_.SetDouble(offset_ + 8, value); }
            
            get { return data_.GetDouble(offset_ + 8); }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct Level2ValueArray
    {
        public Level2ValueArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 16);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public Level2Value this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 16);
                
                return new Level2Value(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct Level2Collection
    {
        public Level2Collection(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public Level2ValueArray Bids
        {
            get { return new Level2ValueArray(data_, offset_ + 0); }
        }
        
        public Level2ValueArray Asks
        {
            get { return new Level2ValueArray(data_, offset_ + 8); }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct Level2CollectionArray
    {
        public Level2CollectionArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 16);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public Level2Collection this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 16);
                
                return new Level2Collection(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct Tick
    {
        public Tick(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public TickId Id
        {
            get { return new TickId(data_, offset_ + 0); }
        }
        
        public Level2Collection Level2
        {
            get { return new Level2Collection(data_, offset_ + 9); }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct TickArray
    {
        public TickArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 25);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public Tick this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 25);
                
                return new Tick(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct QuoteHistorySymbolsRequest
    {
        public static implicit operator Message(QuoteHistorySymbolsRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public QuoteHistorySymbolsRequest(int i)
        {
            info_ = QuoteHistoryCacheProtocol.Info.QuoteHistorySymbolsRequest;
            data_ = new MessageData(16);
            
            data_.SetInt(4, 0);
        }
        
        public QuoteHistorySymbolsRequest(MessageInfo info, MessageData data)
        {
            if (! info.Is(QuoteHistoryCacheProtocol.Info.QuoteHistorySymbolsRequest))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public string RequestId
        {
            set { data_.SetUString(8, value); }
            
            get { return data_.GetUString(8); }
        }
        
        public int GetRequestIdLength()
        {
            return data_.GetUStringLength(8);
        }
        
        public void SetRequestId(char[] value, int offset, int count)
        {
            data_.SetUString(8, value, offset, count);
        }
        
        public void GetRequestId(char[] value, int offset)
        {
            data_.GetUString(8, value, offset);
        }
        
        public void ReadRequestId(Stream stream, int size)
        {
            data_.ReadUString(8, stream, size);
        }
        
        public void WriteRequestId(Stream stream)
        {
            data_.WriteUString(8, stream);
        }
        
        public QuoteHistorySymbolsRequest Clone()
        {
            MessageData data = data_.Clone();
            
            return new QuoteHistorySymbolsRequest(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(16);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct QuoteHistorySymbolsReport
    {
        public static implicit operator Message(QuoteHistorySymbolsReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public QuoteHistorySymbolsReport(int i)
        {
            info_ = QuoteHistoryCacheProtocol.Info.QuoteHistorySymbolsReport;
            data_ = new MessageData(24);
            
            data_.SetInt(4, 1);
        }
        
        public QuoteHistorySymbolsReport(MessageInfo info, MessageData data)
        {
            if (! info.Is(QuoteHistoryCacheProtocol.Info.QuoteHistorySymbolsReport))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public string RequestId
        {
            set { data_.SetUString(8, value); }
            
            get { return data_.GetUString(8); }
        }
        
        public int GetRequestIdLength()
        {
            return data_.GetUStringLength(8);
        }
        
        public void SetRequestId(char[] value, int offset, int count)
        {
            data_.SetUString(8, value, offset, count);
        }
        
        public void GetRequestId(char[] value, int offset)
        {
            data_.GetUString(8, value, offset);
        }
        
        public void ReadRequestId(Stream stream, int size)
        {
            data_.ReadUString(8, stream, size);
        }
        
        public void WriteRequestId(Stream stream)
        {
            data_.WriteUString(8, stream);
        }
        
        public UStringArray Symbols
        {
            get { return new UStringArray(data_, 16); }
        }
        
        public QuoteHistorySymbolsReport Clone()
        {
            MessageData data = data_.Clone();
            
            return new QuoteHistorySymbolsReport(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(24);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct QuoteHistoryPeriodicitiesRequest
    {
        public static implicit operator Message(QuoteHistoryPeriodicitiesRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public QuoteHistoryPeriodicitiesRequest(int i)
        {
            info_ = QuoteHistoryCacheProtocol.Info.QuoteHistoryPeriodicitiesRequest;
            data_ = new MessageData(16);
            
            data_.SetInt(4, 2);
        }
        
        public QuoteHistoryPeriodicitiesRequest(MessageInfo info, MessageData data)
        {
            if (! info.Is(QuoteHistoryCacheProtocol.Info.QuoteHistoryPeriodicitiesRequest))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public string RequestId
        {
            set { data_.SetUString(8, value); }
            
            get { return data_.GetUString(8); }
        }
        
        public int GetRequestIdLength()
        {
            return data_.GetUStringLength(8);
        }
        
        public void SetRequestId(char[] value, int offset, int count)
        {
            data_.SetUString(8, value, offset, count);
        }
        
        public void GetRequestId(char[] value, int offset)
        {
            data_.GetUString(8, value, offset);
        }
        
        public void ReadRequestId(Stream stream, int size)
        {
            data_.ReadUString(8, stream, size);
        }
        
        public void WriteRequestId(Stream stream)
        {
            data_.WriteUString(8, stream);
        }
        
        public QuoteHistoryPeriodicitiesRequest Clone()
        {
            MessageData data = data_.Clone();
            
            return new QuoteHistoryPeriodicitiesRequest(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(16);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct QuoteHistoryPeriodicitiesReport
    {
        public static implicit operator Message(QuoteHistoryPeriodicitiesReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public QuoteHistoryPeriodicitiesReport(int i)
        {
            info_ = QuoteHistoryCacheProtocol.Info.QuoteHistoryPeriodicitiesReport;
            data_ = new MessageData(24);
            
            data_.SetInt(4, 3);
        }
        
        public QuoteHistoryPeriodicitiesReport(MessageInfo info, MessageData data)
        {
            if (! info.Is(QuoteHistoryCacheProtocol.Info.QuoteHistoryPeriodicitiesReport))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public string RequestId
        {
            set { data_.SetUString(8, value); }
            
            get { return data_.GetUString(8); }
        }
        
        public int GetRequestIdLength()
        {
            return data_.GetUStringLength(8);
        }
        
        public void SetRequestId(char[] value, int offset, int count)
        {
            data_.SetUString(8, value, offset, count);
        }
        
        public void GetRequestId(char[] value, int offset)
        {
            data_.GetUString(8, value, offset);
        }
        
        public void ReadRequestId(Stream stream, int size)
        {
            data_.ReadUString(8, stream, size);
        }
        
        public void WriteRequestId(Stream stream)
        {
            data_.WriteUString(8, stream);
        }
        
        public UStringArray Periodicities
        {
            get { return new UStringArray(data_, 16); }
        }
        
        public QuoteHistoryPeriodicitiesReport Clone()
        {
            MessageData data = data_.Clone();
            
            return new QuoteHistoryPeriodicitiesReport(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(24);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct QueryQuoteHistoryBarsRequest
    {
        public static implicit operator Message(QueryQuoteHistoryBarsRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public QueryQuoteHistoryBarsRequest(int i)
        {
            info_ = QuoteHistoryCacheProtocol.Info.QueryQuoteHistoryBarsRequest;
            data_ = new MessageData(48);
            
            data_.SetInt(4, 4);
        }
        
        public QueryQuoteHistoryBarsRequest(MessageInfo info, MessageData data)
        {
            if (! info.Is(QuoteHistoryCacheProtocol.Info.QueryQuoteHistoryBarsRequest))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public string RequestId
        {
            set { data_.SetUString(8, value); }
            
            get { return data_.GetUString(8); }
        }
        
        public int GetRequestIdLength()
        {
            return data_.GetUStringLength(8);
        }
        
        public void SetRequestId(char[] value, int offset, int count)
        {
            data_.SetUString(8, value, offset, count);
        }
        
        public void GetRequestId(char[] value, int offset)
        {
            data_.GetUString(8, value, offset);
        }
        
        public void ReadRequestId(Stream stream, int size)
        {
            data_.ReadUString(8, stream, size);
        }
        
        public void WriteRequestId(Stream stream)
        {
            data_.WriteUString(8, stream);
        }
        
        public DateTime Timestamp
        {
            set { data_.SetDateTime(16, value); }
            
            get { return data_.GetDateTime(16); }
        }
        
        public int Count
        {
            set { data_.SetInt(24, value); }
            
            get { return data_.GetInt(24); }
        }
        
        public string Symbol
        {
            set { data_.SetUString(28, value); }
            
            get { return data_.GetUString(28); }
        }
        
        public int GetSymbolLength()
        {
            return data_.GetUStringLength(28);
        }
        
        public void SetSymbol(char[] value, int offset, int count)
        {
            data_.SetUString(28, value, offset, count);
        }
        
        public void GetSymbol(char[] value, int offset)
        {
            data_.GetUString(28, value, offset);
        }
        
        public void ReadSymbol(Stream stream, int size)
        {
            data_.ReadUString(28, stream, size);
        }
        
        public void WriteSymbol(Stream stream)
        {
            data_.WriteUString(28, stream);
        }
        
        public string Periodicity
        {
            set { data_.SetUString(36, value); }
            
            get { return data_.GetUString(36); }
        }
        
        public int GetPeriodicityLength()
        {
            return data_.GetUStringLength(36);
        }
        
        public void SetPeriodicity(char[] value, int offset, int count)
        {
            data_.SetUString(36, value, offset, count);
        }
        
        public void GetPeriodicity(char[] value, int offset)
        {
            data_.GetUString(36, value, offset);
        }
        
        public void ReadPeriodicity(Stream stream, int size)
        {
            data_.ReadUString(36, stream, size);
        }
        
        public void WritePeriodicity(Stream stream)
        {
            data_.WriteUString(36, stream);
        }
        
        public PriceType PriceType
        {
            set { data_.SetUInt(44, (uint) value); }
            
            get { return (PriceType) data_.GetUInt(44); }
        }
        
        public QueryQuoteHistoryBarsRequest Clone()
        {
            MessageData data = data_.Clone();
            
            return new QueryQuoteHistoryBarsRequest(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(48);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct QueryQuoteHistoryBarsReport
    {
        public static implicit operator Message(QueryQuoteHistoryBarsReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public QueryQuoteHistoryBarsReport(int i)
        {
            info_ = QuoteHistoryCacheProtocol.Info.QueryQuoteHistoryBarsReport;
            data_ = new MessageData(24);
            
            data_.SetInt(4, 5);
        }
        
        public QueryQuoteHistoryBarsReport(MessageInfo info, MessageData data)
        {
            if (! info.Is(QuoteHistoryCacheProtocol.Info.QueryQuoteHistoryBarsReport))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public string RequestId
        {
            set { data_.SetUString(8, value); }
            
            get { return data_.GetUString(8); }
        }
        
        public int GetRequestIdLength()
        {
            return data_.GetUStringLength(8);
        }
        
        public void SetRequestId(char[] value, int offset, int count)
        {
            data_.SetUString(8, value, offset, count);
        }
        
        public void GetRequestId(char[] value, int offset)
        {
            data_.GetUString(8, value, offset);
        }
        
        public void ReadRequestId(Stream stream, int size)
        {
            data_.ReadUString(8, stream, size);
        }
        
        public void WriteRequestId(Stream stream)
        {
            data_.WriteUString(8, stream);
        }
        
        public BarArray Bars
        {
            get { return new BarArray(data_, 16); }
        }
        
        public QueryQuoteHistoryBarsReport Clone()
        {
            MessageData data = data_.Clone();
            
            return new QueryQuoteHistoryBarsReport(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(24);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct QueryQuoteHistoryTicksRequest
    {
        public static implicit operator Message(QueryQuoteHistoryTicksRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public QueryQuoteHistoryTicksRequest(int i)
        {
            info_ = QuoteHistoryCacheProtocol.Info.QueryQuoteHistoryTicksRequest;
            data_ = new MessageData(37);
            
            data_.SetInt(4, 6);
        }
        
        public QueryQuoteHistoryTicksRequest(MessageInfo info, MessageData data)
        {
            if (! info.Is(QuoteHistoryCacheProtocol.Info.QueryQuoteHistoryTicksRequest))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public string RequestId
        {
            set { data_.SetUString(8, value); }
            
            get { return data_.GetUString(8); }
        }
        
        public int GetRequestIdLength()
        {
            return data_.GetUStringLength(8);
        }
        
        public void SetRequestId(char[] value, int offset, int count)
        {
            data_.SetUString(8, value, offset, count);
        }
        
        public void GetRequestId(char[] value, int offset)
        {
            data_.GetUString(8, value, offset);
        }
        
        public void ReadRequestId(Stream stream, int size)
        {
            data_.ReadUString(8, stream, size);
        }
        
        public void WriteRequestId(Stream stream)
        {
            data_.WriteUString(8, stream);
        }
        
        public DateTime Timestamp
        {
            set { data_.SetDateTime(16, value); }
            
            get { return data_.GetDateTime(16); }
        }
        
        public int Count
        {
            set { data_.SetInt(24, value); }
            
            get { return data_.GetInt(24); }
        }
        
        public string Symbol
        {
            set { data_.SetUString(28, value); }
            
            get { return data_.GetUString(28); }
        }
        
        public int GetSymbolLength()
        {
            return data_.GetUStringLength(28);
        }
        
        public void SetSymbol(char[] value, int offset, int count)
        {
            data_.SetUString(28, value, offset, count);
        }
        
        public void GetSymbol(char[] value, int offset)
        {
            data_.GetUString(28, value, offset);
        }
        
        public void ReadSymbol(Stream stream, int size)
        {
            data_.ReadUString(28, stream, size);
        }
        
        public void WriteSymbol(Stream stream)
        {
            data_.WriteUString(28, stream);
        }
        
        public bool Level2
        {
            set { data_.SetBool(36, value); }
            
            get { return data_.GetBool(36); }
        }
        
        public QueryQuoteHistoryTicksRequest Clone()
        {
            MessageData data = data_.Clone();
            
            return new QueryQuoteHistoryTicksRequest(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(37);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct QueryQuoteHistoryTicksReport
    {
        public static implicit operator Message(QueryQuoteHistoryTicksReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public QueryQuoteHistoryTicksReport(int i)
        {
            info_ = QuoteHistoryCacheProtocol.Info.QueryQuoteHistoryTicksReport;
            data_ = new MessageData(24);
            
            data_.SetInt(4, 7);
        }
        
        public QueryQuoteHistoryTicksReport(MessageInfo info, MessageData data)
        {
            if (! info.Is(QuoteHistoryCacheProtocol.Info.QueryQuoteHistoryTicksReport))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public string RequestId
        {
            set { data_.SetUString(8, value); }
            
            get { return data_.GetUString(8); }
        }
        
        public int GetRequestIdLength()
        {
            return data_.GetUStringLength(8);
        }
        
        public void SetRequestId(char[] value, int offset, int count)
        {
            data_.SetUString(8, value, offset, count);
        }
        
        public void GetRequestId(char[] value, int offset)
        {
            data_.GetUString(8, value, offset);
        }
        
        public void ReadRequestId(Stream stream, int size)
        {
            data_.ReadUString(8, stream, size);
        }
        
        public void WriteRequestId(Stream stream)
        {
            data_.WriteUString(8, stream);
        }
        
        public TickArray Ticks
        {
            get { return new TickArray(data_, 16); }
        }
        
        public QueryQuoteHistoryTicksReport Clone()
        {
            MessageData data = data_.Clone();
            
            return new QueryQuoteHistoryTicksReport(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(24);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct QueryQuoteHistoryReject
    {
        public static implicit operator Message(QueryQuoteHistoryReject message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public QueryQuoteHistoryReject(int i)
        {
            info_ = QuoteHistoryCacheProtocol.Info.QueryQuoteHistoryReject;
            data_ = new MessageData(24);
            
            data_.SetInt(4, 8);
        }
        
        public QueryQuoteHistoryReject(MessageInfo info, MessageData data)
        {
            if (! info.Is(QuoteHistoryCacheProtocol.Info.QueryQuoteHistoryReject))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public string RequestId
        {
            set { data_.SetUString(8, value); }
            
            get { return data_.GetUString(8); }
        }
        
        public int GetRequestIdLength()
        {
            return data_.GetUStringLength(8);
        }
        
        public void SetRequestId(char[] value, int offset, int count)
        {
            data_.SetUString(8, value, offset, count);
        }
        
        public void GetRequestId(char[] value, int offset)
        {
            data_.GetUString(8, value, offset);
        }
        
        public void ReadRequestId(Stream stream, int size)
        {
            data_.ReadUString(8, stream, size);
        }
        
        public void WriteRequestId(Stream stream)
        {
            data_.WriteUString(8, stream);
        }
        
        public string RejectMessage
        {
            set { data_.SetUString(16, value); }
            
            get { return data_.GetUString(16); }
        }
        
        public int GetRejectMessageLength()
        {
            return data_.GetUStringLength(16);
        }
        
        public void SetRejectMessage(char[] value, int offset, int count)
        {
            data_.SetUString(16, value, offset, count);
        }
        
        public void GetRejectMessage(char[] value, int offset)
        {
            data_.GetUString(16, value, offset);
        }
        
        public void ReadRejectMessage(Stream stream, int size)
        {
            data_.ReadUString(16, stream, size);
        }
        
        public void WriteRejectMessage(Stream stream)
        {
            data_.WriteUString(16, stream);
        }
        
        public QueryQuoteHistoryReject Clone()
        {
            MessageData data = data_.Clone();
            
            return new QueryQuoteHistoryReject(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(24);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    class Is
    {
        public static bool QuoteHistorySymbolsRequest(Message message)
        {
            return message.Info.Is(Info.QuoteHistorySymbolsRequest);
        }
        
        public static bool QuoteHistorySymbolsReport(Message message)
        {
            return message.Info.Is(Info.QuoteHistorySymbolsReport);
        }
        
        public static bool QuoteHistoryPeriodicitiesRequest(Message message)
        {
            return message.Info.Is(Info.QuoteHistoryPeriodicitiesRequest);
        }
        
        public static bool QuoteHistoryPeriodicitiesReport(Message message)
        {
            return message.Info.Is(Info.QuoteHistoryPeriodicitiesReport);
        }
        
        public static bool QueryQuoteHistoryBarsRequest(Message message)
        {
            return message.Info.Is(Info.QueryQuoteHistoryBarsRequest);
        }
        
        public static bool QueryQuoteHistoryBarsReport(Message message)
        {
            return message.Info.Is(Info.QueryQuoteHistoryBarsReport);
        }
        
        public static bool QueryQuoteHistoryTicksRequest(Message message)
        {
            return message.Info.Is(Info.QueryQuoteHistoryTicksRequest);
        }
        
        public static bool QueryQuoteHistoryTicksReport(Message message)
        {
            return message.Info.Is(Info.QueryQuoteHistoryTicksReport);
        }
        
        public static bool QueryQuoteHistoryReject(Message message)
        {
            return message.Info.Is(Info.QueryQuoteHistoryReject);
        }
    }
    
    class Cast
    {
        public static Message Message(QuoteHistorySymbolsRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static QuoteHistorySymbolsRequest QuoteHistorySymbolsRequest(Message message)
        {
            return new QuoteHistorySymbolsRequest(message.Info, message.Data);
        }
        
        public static Message Message(QuoteHistorySymbolsReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static QuoteHistorySymbolsReport QuoteHistorySymbolsReport(Message message)
        {
            return new QuoteHistorySymbolsReport(message.Info, message.Data);
        }
        
        public static Message Message(QuoteHistoryPeriodicitiesRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static QuoteHistoryPeriodicitiesRequest QuoteHistoryPeriodicitiesRequest(Message message)
        {
            return new QuoteHistoryPeriodicitiesRequest(message.Info, message.Data);
        }
        
        public static Message Message(QuoteHistoryPeriodicitiesReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static QuoteHistoryPeriodicitiesReport QuoteHistoryPeriodicitiesReport(Message message)
        {
            return new QuoteHistoryPeriodicitiesReport(message.Info, message.Data);
        }
        
        public static Message Message(QueryQuoteHistoryBarsRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static QueryQuoteHistoryBarsRequest QueryQuoteHistoryBarsRequest(Message message)
        {
            return new QueryQuoteHistoryBarsRequest(message.Info, message.Data);
        }
        
        public static Message Message(QueryQuoteHistoryBarsReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static QueryQuoteHistoryBarsReport QueryQuoteHistoryBarsReport(Message message)
        {
            return new QueryQuoteHistoryBarsReport(message.Info, message.Data);
        }
        
        public static Message Message(QueryQuoteHistoryTicksRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static QueryQuoteHistoryTicksRequest QueryQuoteHistoryTicksRequest(Message message)
        {
            return new QueryQuoteHistoryTicksRequest(message.Info, message.Data);
        }
        
        public static Message Message(QueryQuoteHistoryTicksReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static QueryQuoteHistoryTicksReport QueryQuoteHistoryTicksReport(Message message)
        {
            return new QueryQuoteHistoryTicksReport(message.Info, message.Data);
        }
        
        public static Message Message(QueryQuoteHistoryReject message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static QueryQuoteHistoryReject QueryQuoteHistoryReject(Message message)
        {
            return new QueryQuoteHistoryReject(message.Info, message.Data);
        }
    }
    
    class Info
    {
        static Info()
        {
            ConstructPriceType();
            ConstructBar();
            ConstructTickId();
            ConstructLevel2Value();
            ConstructLevel2Collection();
            ConstructTick();
            ConstructQuoteHistorySymbolsRequest();
            ConstructQuoteHistorySymbolsReport();
            ConstructQuoteHistoryPeriodicitiesRequest();
            ConstructQuoteHistoryPeriodicitiesReport();
            ConstructQueryQuoteHistoryBarsRequest();
            ConstructQueryQuoteHistoryBarsReport();
            ConstructQueryQuoteHistoryTicksRequest();
            ConstructQueryQuoteHistoryTicksReport();
            ConstructQueryQuoteHistoryReject();
            ConstructQuoteHistoryCacheProtocol();
        }
        
        static void ConstructPriceType()
        {
            EnumValue Bid = new EnumValue();
            Bid.name = "Bid";
            Bid.value = 0;
            
            EnumValue Ask = new EnumValue();
            Ask.name = "Ask";
            Ask.value = 1;
            
            PriceType = new EnumInfo();
            PriceType.name = "PriceType";
            PriceType.minSize = 4;
            PriceType.values = new EnumValue[2];
            PriceType.values[0] = Bid;
            PriceType.values[1] = Ask;
        }
        
        static void ConstructBar()
        {
            FieldInfo Time = new FieldInfo();
            Time.name = "Time";
            Time.offset = 0;
            Time.type = FieldType.DateTime;
            Time.optional = false;
            Time.repeatable = false;
            
            FieldInfo Open = new FieldInfo();
            Open.name = "Open";
            Open.offset = 8;
            Open.type = FieldType.Double;
            Open.optional = false;
            Open.repeatable = false;
            
            FieldInfo High = new FieldInfo();
            High.name = "High";
            High.offset = 16;
            High.type = FieldType.Double;
            High.optional = false;
            High.repeatable = false;
            
            FieldInfo Low = new FieldInfo();
            Low.name = "Low";
            Low.offset = 24;
            Low.type = FieldType.Double;
            Low.optional = false;
            Low.repeatable = false;
            
            FieldInfo Close = new FieldInfo();
            Close.name = "Close";
            Close.offset = 32;
            Close.type = FieldType.Double;
            Close.optional = false;
            Close.repeatable = false;
            
            FieldInfo Volume = new FieldInfo();
            Volume.name = "Volume";
            Volume.offset = 40;
            Volume.type = FieldType.Double;
            Volume.optional = false;
            Volume.repeatable = false;
            
            Bar = new GroupInfo();
            Bar.name = "Bar";
            Bar.minSize = 48;
            Bar.fields = new FieldInfo[6];
            Bar.fields[0] = Time;
            Bar.fields[1] = Open;
            Bar.fields[2] = High;
            Bar.fields[3] = Low;
            Bar.fields[4] = Close;
            Bar.fields[5] = Volume;
        }
        
        static void ConstructTickId()
        {
            FieldInfo Time = new FieldInfo();
            Time.name = "Time";
            Time.offset = 0;
            Time.type = FieldType.DateTime;
            Time.optional = false;
            Time.repeatable = false;
            
            FieldInfo Index = new FieldInfo();
            Index.name = "Index";
            Index.offset = 8;
            Index.type = FieldType.Byte;
            Index.optional = false;
            Index.repeatable = false;
            
            TickId = new GroupInfo();
            TickId.name = "TickId";
            TickId.minSize = 9;
            TickId.fields = new FieldInfo[2];
            TickId.fields[0] = Time;
            TickId.fields[1] = Index;
        }
        
        static void ConstructLevel2Value()
        {
            FieldInfo Price = new FieldInfo();
            Price.name = "Price";
            Price.offset = 0;
            Price.type = FieldType.Double;
            Price.optional = false;
            Price.repeatable = false;
            
            FieldInfo Volume = new FieldInfo();
            Volume.name = "Volume";
            Volume.offset = 8;
            Volume.type = FieldType.Double;
            Volume.optional = false;
            Volume.repeatable = false;
            
            Level2Value = new GroupInfo();
            Level2Value.name = "Level2Value";
            Level2Value.minSize = 16;
            Level2Value.fields = new FieldInfo[2];
            Level2Value.fields[0] = Price;
            Level2Value.fields[1] = Volume;
        }
        
        static void ConstructLevel2Collection()
        {
            FieldInfo Bids = new FieldInfo();
            Bids.name = "Bids";
            Bids.offset = 0;
            Bids.type = FieldType.Group;
            Bids.groupInfo = Info.Level2Value;
            Bids.optional = false;
            Bids.repeatable = true;
            
            FieldInfo Asks = new FieldInfo();
            Asks.name = "Asks";
            Asks.offset = 8;
            Asks.type = FieldType.Group;
            Asks.groupInfo = Info.Level2Value;
            Asks.optional = false;
            Asks.repeatable = true;
            
            Level2Collection = new GroupInfo();
            Level2Collection.name = "Level2Collection";
            Level2Collection.minSize = 16;
            Level2Collection.fields = new FieldInfo[2];
            Level2Collection.fields[0] = Bids;
            Level2Collection.fields[1] = Asks;
        }
        
        static void ConstructTick()
        {
            FieldInfo Id = new FieldInfo();
            Id.name = "Id";
            Id.offset = 0;
            Id.type = FieldType.Group;
            Id.groupInfo = Info.TickId;
            Id.optional = false;
            Id.repeatable = false;
            
            FieldInfo Level2 = new FieldInfo();
            Level2.name = "Level2";
            Level2.offset = 9;
            Level2.type = FieldType.Group;
            Level2.groupInfo = Info.Level2Collection;
            Level2.optional = false;
            Level2.repeatable = false;
            
            Tick = new GroupInfo();
            Tick.name = "Tick";
            Tick.minSize = 25;
            Tick.fields = new FieldInfo[2];
            Tick.fields[0] = Id;
            Tick.fields[1] = Level2;
        }
        
        static void ConstructQuoteHistorySymbolsRequest()
        {
            FieldInfo RequestId = new FieldInfo();
            RequestId.name = "RequestId";
            RequestId.offset = 8;
            RequestId.type = FieldType.UString;
            RequestId.optional = false;
            RequestId.repeatable = false;
            
            QuoteHistorySymbolsRequest = new MessageInfo();
            QuoteHistorySymbolsRequest.name = "QuoteHistorySymbolsRequest";
            QuoteHistorySymbolsRequest.id = 0;
            QuoteHistorySymbolsRequest.minSize = 16;
            QuoteHistorySymbolsRequest.fields = new FieldInfo[1];
            QuoteHistorySymbolsRequest.fields[0] = RequestId;
        }
        
        static void ConstructQuoteHistorySymbolsReport()
        {
            FieldInfo RequestId = new FieldInfo();
            RequestId.name = "RequestId";
            RequestId.offset = 8;
            RequestId.type = FieldType.UString;
            RequestId.optional = false;
            RequestId.repeatable = false;
            
            FieldInfo Symbols = new FieldInfo();
            Symbols.name = "Symbols";
            Symbols.offset = 16;
            Symbols.type = FieldType.UString;
            Symbols.optional = false;
            Symbols.repeatable = true;
            
            QuoteHistorySymbolsReport = new MessageInfo();
            QuoteHistorySymbolsReport.name = "QuoteHistorySymbolsReport";
            QuoteHistorySymbolsReport.id = 1;
            QuoteHistorySymbolsReport.minSize = 24;
            QuoteHistorySymbolsReport.fields = new FieldInfo[2];
            QuoteHistorySymbolsReport.fields[0] = RequestId;
            QuoteHistorySymbolsReport.fields[1] = Symbols;
        }
        
        static void ConstructQuoteHistoryPeriodicitiesRequest()
        {
            FieldInfo RequestId = new FieldInfo();
            RequestId.name = "RequestId";
            RequestId.offset = 8;
            RequestId.type = FieldType.UString;
            RequestId.optional = false;
            RequestId.repeatable = false;
            
            QuoteHistoryPeriodicitiesRequest = new MessageInfo();
            QuoteHistoryPeriodicitiesRequest.name = "QuoteHistoryPeriodicitiesRequest";
            QuoteHistoryPeriodicitiesRequest.id = 2;
            QuoteHistoryPeriodicitiesRequest.minSize = 16;
            QuoteHistoryPeriodicitiesRequest.fields = new FieldInfo[1];
            QuoteHistoryPeriodicitiesRequest.fields[0] = RequestId;
        }
        
        static void ConstructQuoteHistoryPeriodicitiesReport()
        {
            FieldInfo RequestId = new FieldInfo();
            RequestId.name = "RequestId";
            RequestId.offset = 8;
            RequestId.type = FieldType.UString;
            RequestId.optional = false;
            RequestId.repeatable = false;
            
            FieldInfo Periodicities = new FieldInfo();
            Periodicities.name = "Periodicities";
            Periodicities.offset = 16;
            Periodicities.type = FieldType.UString;
            Periodicities.optional = false;
            Periodicities.repeatable = true;
            
            QuoteHistoryPeriodicitiesReport = new MessageInfo();
            QuoteHistoryPeriodicitiesReport.name = "QuoteHistoryPeriodicitiesReport";
            QuoteHistoryPeriodicitiesReport.id = 3;
            QuoteHistoryPeriodicitiesReport.minSize = 24;
            QuoteHistoryPeriodicitiesReport.fields = new FieldInfo[2];
            QuoteHistoryPeriodicitiesReport.fields[0] = RequestId;
            QuoteHistoryPeriodicitiesReport.fields[1] = Periodicities;
        }
        
        static void ConstructQueryQuoteHistoryBarsRequest()
        {
            FieldInfo RequestId = new FieldInfo();
            RequestId.name = "RequestId";
            RequestId.offset = 8;
            RequestId.type = FieldType.UString;
            RequestId.optional = false;
            RequestId.repeatable = false;
            
            FieldInfo Timestamp = new FieldInfo();
            Timestamp.name = "Timestamp";
            Timestamp.offset = 16;
            Timestamp.type = FieldType.DateTime;
            Timestamp.optional = false;
            Timestamp.repeatable = false;
            
            FieldInfo Count = new FieldInfo();
            Count.name = "Count";
            Count.offset = 24;
            Count.type = FieldType.Int;
            Count.optional = false;
            Count.repeatable = false;
            
            FieldInfo Symbol = new FieldInfo();
            Symbol.name = "Symbol";
            Symbol.offset = 28;
            Symbol.type = FieldType.UString;
            Symbol.optional = false;
            Symbol.repeatable = false;
            
            FieldInfo Periodicity = new FieldInfo();
            Periodicity.name = "Periodicity";
            Periodicity.offset = 36;
            Periodicity.type = FieldType.UString;
            Periodicity.optional = false;
            Periodicity.repeatable = false;
            
            FieldInfo PriceType = new FieldInfo();
            PriceType.name = "PriceType";
            PriceType.offset = 44;
            PriceType.type = FieldType.Enum;
            PriceType.enumInfo = Info.PriceType;
            PriceType.optional = false;
            PriceType.repeatable = false;
            
            QueryQuoteHistoryBarsRequest = new MessageInfo();
            QueryQuoteHistoryBarsRequest.name = "QueryQuoteHistoryBarsRequest";
            QueryQuoteHistoryBarsRequest.id = 4;
            QueryQuoteHistoryBarsRequest.minSize = 48;
            QueryQuoteHistoryBarsRequest.fields = new FieldInfo[6];
            QueryQuoteHistoryBarsRequest.fields[0] = RequestId;
            QueryQuoteHistoryBarsRequest.fields[1] = Timestamp;
            QueryQuoteHistoryBarsRequest.fields[2] = Count;
            QueryQuoteHistoryBarsRequest.fields[3] = Symbol;
            QueryQuoteHistoryBarsRequest.fields[4] = Periodicity;
            QueryQuoteHistoryBarsRequest.fields[5] = PriceType;
        }
        
        static void ConstructQueryQuoteHistoryBarsReport()
        {
            FieldInfo RequestId = new FieldInfo();
            RequestId.name = "RequestId";
            RequestId.offset = 8;
            RequestId.type = FieldType.UString;
            RequestId.optional = false;
            RequestId.repeatable = false;
            
            FieldInfo Bars = new FieldInfo();
            Bars.name = "Bars";
            Bars.offset = 16;
            Bars.type = FieldType.Group;
            Bars.groupInfo = Info.Bar;
            Bars.optional = false;
            Bars.repeatable = true;
            
            QueryQuoteHistoryBarsReport = new MessageInfo();
            QueryQuoteHistoryBarsReport.name = "QueryQuoteHistoryBarsReport";
            QueryQuoteHistoryBarsReport.id = 5;
            QueryQuoteHistoryBarsReport.minSize = 24;
            QueryQuoteHistoryBarsReport.fields = new FieldInfo[2];
            QueryQuoteHistoryBarsReport.fields[0] = RequestId;
            QueryQuoteHistoryBarsReport.fields[1] = Bars;
        }
        
        static void ConstructQueryQuoteHistoryTicksRequest()
        {
            FieldInfo RequestId = new FieldInfo();
            RequestId.name = "RequestId";
            RequestId.offset = 8;
            RequestId.type = FieldType.UString;
            RequestId.optional = false;
            RequestId.repeatable = false;
            
            FieldInfo Timestamp = new FieldInfo();
            Timestamp.name = "Timestamp";
            Timestamp.offset = 16;
            Timestamp.type = FieldType.DateTime;
            Timestamp.optional = false;
            Timestamp.repeatable = false;
            
            FieldInfo Count = new FieldInfo();
            Count.name = "Count";
            Count.offset = 24;
            Count.type = FieldType.Int;
            Count.optional = false;
            Count.repeatable = false;
            
            FieldInfo Symbol = new FieldInfo();
            Symbol.name = "Symbol";
            Symbol.offset = 28;
            Symbol.type = FieldType.UString;
            Symbol.optional = false;
            Symbol.repeatable = false;
            
            FieldInfo Level2 = new FieldInfo();
            Level2.name = "Level2";
            Level2.offset = 36;
            Level2.type = FieldType.Bool;
            Level2.optional = false;
            Level2.repeatable = false;
            
            QueryQuoteHistoryTicksRequest = new MessageInfo();
            QueryQuoteHistoryTicksRequest.name = "QueryQuoteHistoryTicksRequest";
            QueryQuoteHistoryTicksRequest.id = 6;
            QueryQuoteHistoryTicksRequest.minSize = 37;
            QueryQuoteHistoryTicksRequest.fields = new FieldInfo[5];
            QueryQuoteHistoryTicksRequest.fields[0] = RequestId;
            QueryQuoteHistoryTicksRequest.fields[1] = Timestamp;
            QueryQuoteHistoryTicksRequest.fields[2] = Count;
            QueryQuoteHistoryTicksRequest.fields[3] = Symbol;
            QueryQuoteHistoryTicksRequest.fields[4] = Level2;
        }
        
        static void ConstructQueryQuoteHistoryTicksReport()
        {
            FieldInfo RequestId = new FieldInfo();
            RequestId.name = "RequestId";
            RequestId.offset = 8;
            RequestId.type = FieldType.UString;
            RequestId.optional = false;
            RequestId.repeatable = false;
            
            FieldInfo Ticks = new FieldInfo();
            Ticks.name = "Ticks";
            Ticks.offset = 16;
            Ticks.type = FieldType.Group;
            Ticks.groupInfo = Info.Tick;
            Ticks.optional = false;
            Ticks.repeatable = true;
            
            QueryQuoteHistoryTicksReport = new MessageInfo();
            QueryQuoteHistoryTicksReport.name = "QueryQuoteHistoryTicksReport";
            QueryQuoteHistoryTicksReport.id = 7;
            QueryQuoteHistoryTicksReport.minSize = 24;
            QueryQuoteHistoryTicksReport.fields = new FieldInfo[2];
            QueryQuoteHistoryTicksReport.fields[0] = RequestId;
            QueryQuoteHistoryTicksReport.fields[1] = Ticks;
        }
        
        static void ConstructQueryQuoteHistoryReject()
        {
            FieldInfo RequestId = new FieldInfo();
            RequestId.name = "RequestId";
            RequestId.offset = 8;
            RequestId.type = FieldType.UString;
            RequestId.optional = false;
            RequestId.repeatable = false;
            
            FieldInfo RejectMessage = new FieldInfo();
            RejectMessage.name = "RejectMessage";
            RejectMessage.offset = 16;
            RejectMessage.type = FieldType.UString;
            RejectMessage.optional = false;
            RejectMessage.repeatable = false;
            
            QueryQuoteHistoryReject = new MessageInfo();
            QueryQuoteHistoryReject.name = "QueryQuoteHistoryReject";
            QueryQuoteHistoryReject.id = 8;
            QueryQuoteHistoryReject.minSize = 24;
            QueryQuoteHistoryReject.fields = new FieldInfo[2];
            QueryQuoteHistoryReject.fields[0] = RequestId;
            QueryQuoteHistoryReject.fields[1] = RejectMessage;
        }
        
        static void ConstructQuoteHistoryCacheProtocol()
        {
            QuoteHistoryCacheProtocol = new ProtocolInfo();
            QuoteHistoryCacheProtocol.name = "QuoteHistoryCacheProtocol";
            QuoteHistoryCacheProtocol.majorVersion = 0;
            QuoteHistoryCacheProtocol.minorVersion = 0;
            QuoteHistoryCacheProtocol.AddMessageInfo(QuoteHistorySymbolsRequest);
            QuoteHistoryCacheProtocol.AddMessageInfo(QuoteHistorySymbolsReport);
            QuoteHistoryCacheProtocol.AddMessageInfo(QuoteHistoryPeriodicitiesRequest);
            QuoteHistoryCacheProtocol.AddMessageInfo(QuoteHistoryPeriodicitiesReport);
            QuoteHistoryCacheProtocol.AddMessageInfo(QueryQuoteHistoryBarsRequest);
            QuoteHistoryCacheProtocol.AddMessageInfo(QueryQuoteHistoryBarsReport);
            QuoteHistoryCacheProtocol.AddMessageInfo(QueryQuoteHistoryTicksRequest);
            QuoteHistoryCacheProtocol.AddMessageInfo(QueryQuoteHistoryTicksReport);
            QuoteHistoryCacheProtocol.AddMessageInfo(QueryQuoteHistoryReject);
        }
        
        public static EnumInfo PriceType;
        public static GroupInfo Bar;
        public static GroupInfo TickId;
        public static GroupInfo Level2Value;
        public static GroupInfo Level2Collection;
        public static GroupInfo Tick;
        public static MessageInfo QuoteHistorySymbolsRequest;
        public static MessageInfo QuoteHistorySymbolsReport;
        public static MessageInfo QuoteHistoryPeriodicitiesRequest;
        public static MessageInfo QuoteHistoryPeriodicitiesReport;
        public static MessageInfo QueryQuoteHistoryBarsRequest;
        public static MessageInfo QueryQuoteHistoryBarsReport;
        public static MessageInfo QueryQuoteHistoryTicksRequest;
        public static MessageInfo QueryQuoteHistoryTicksReport;
        public static MessageInfo QueryQuoteHistoryReject;
        public static ProtocolInfo QuoteHistoryCacheProtocol;
    }
}
