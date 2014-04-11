using System;
using System.IO.Ports;
using System.Threading;
using DNT.Diag.IO;

namespace DNT.Diag.Commbox.W80
{
    public class W80Commbox : ICommbox
    {
        public const int BOXINFO_LEN = 12;
        public const int MAXPORT_NUM = 4;
        public const int MAXBUFF_NUM = 4;
        public const int MAXBUFF_LEN = 0xA8;
        public const int LINKBLOCK = 0x40;
        // 批处理执行次数
        public const int RUN_ONCE = 0x00;
        public const int RUN_MORE = 0x01;
        // 通讯校验和方式
        public const int CHECK_SUM = 0x01;
        public const int CHECK_REVSUM = 0x02;
        public const int CHECK_CRC = 0x03;
        // /////////////////////////////////////////////////////////////////////////////
        // 通讯口 PORT
        // /////////////////////////////////////////////////////////////////////////////
        public const int DH = 0x80;
        // 高电平输出,1为关闭,0为打开
        public const int DL2 = 0x40;
        // 低电平输出,1为关闭,0为打开,正逻辑发送通讯线
        public const int DL1 = 0x20;
        // 低电平输出,1为关闭,0为打开,正逻辑发送通讯线,带接受控制
        public const int DL0 = 0x10;
        // 低电平输出,1为关闭,0为打开,正逻辑发送通讯线,带接受控制
        public const int PWMS = 0x08;
        // PWM发送线
        public const int PWMR = 0x04;
        public const int COMS = 0x02;
        // 标准发送通讯线路
        public const int COMR = 0x01;
        public const int SET_NULL = 0x00;
        // 不选择任何设置
        // /////////////////////////////////////////////////////////////////////////////
        // 通讯物理控制口
        // /////////////////////////////////////////////////////////////////////////////
        public const int PWC = 0x80;
        // 通讯电平控制,1为5伏,0为12伏
        public const int REFC = 0x40;
        // 通讯比较电平控制,1为通讯电平1/5,0为比较电平控制1/2
        public const int CK = 0x20;
        // K线控制开关,1为双线通讯,0为单线通讯
        public const int SZFC = 0x10;
        // 发送逻辑控制,1为负逻辑,0为正逻辑
        public const int RZFC = 0x08;
        // 接受逻辑控制,1为负逻辑,0为正逻辑
        public const int DLC0 = 0x04;
        // DLC1接受控制,1为接受关闭,0为接受打开
        public const int DLC1 = 0x02;
        // DLC0接受控制,1为接受关闭,0为接受打开
        public const int SLC = 0x01;
        // 线选地址锁存器控制线(待用)
        public const int CLOSEALL = 0x08;
        // 关闭所有发送口线，和接受口线
        // /////////////////////////////////////////////////////////////////////////////
        // 通讯控制字1设定
        // /////////////////////////////////////////////////////////////////////////////
        public const int RS_232 = 0x00;
        public const int EXRS_232 = 0x20;
        public const int SET_VPW = 0x40;
        public const int SET_PWM = 0x60;
        public const int BIT9_SPACE = 0x00;
        public const int BIT9_MARK = 0x01;
        public const int BIT9_EVEN = 0x02;
        public const int BIT9_ODD = 0x03;
        public const int SEL_SL = 0x00;
        public const int SEL_DL0 = 0x08;
        public const int SEL_DL1 = 0x10;
        public const int SEL_DL2 = 0x18;
        public const int SET_DB20 = 0x04;
        public const int UN_DB20 = 0x00;
        // /////////////////////////////////////////////////////////////////////////////
        // 通讯控制字3设定
        // /////////////////////////////////////////////////////////////////////////////
        public const int ONEBYONE = 0x80;
        public const int INVERTBYTE = 0x40;
        public const int ORIGNALBYTE = 0x00;
        // /////////////////////////////////////////////////////////////////////////////
        // 接受命令类型定义
        // /////////////////////////////////////////////////////////////////////////////
        public const int WR_DATA = 0x00;
        public const int WR_LINK = 0xFF;
        public const int STOP_REC = 0x04;
        public const int STOP_EXECUTE = 0x08;
        public const int SET_UPBAUD = 0x0C;
        public const int UP_9600BPS = 0x00;
        public const int UP_19200BPS = 0x01;
        public const int UP_38400BPS = 0x02;
        public const int UP_57600BPS = 0x03;
        public const int UP_115200BPS = 0x04;
        public const int RESET = 0x10;
        public const int GET_CPU = 0x14;
        public const int GET_TIME = 0x18;
        public const int GET_SET = 0x1C;
        public const int GET_LINK = 0x20;
        public const int GET_BUF = 0x24;
        public const int GET_CMD = 0x28;
        public const int GET_PORT = 0x2C;
        public const int GET_BOXID = 0x30;
        public const int DO_BAT_C = 0x34;
        public const int DO_BAT_CN = 0x38;
        public const int DO_BAT_L = 0x3C;
        public const int DO_BAT_LN = 0x40;
        public const int SET55_BAUD = 0x44;
        public const int SET_ONEBYONE = 0x48;
        public const int SET_BAUD = 0x4C;
        public const int RUN_LINK = 0x50;
        public const int STOP_LINK = 0x54;
        public const int CLEAR_LINK = 0x58;
        public const int GET_PORT1 = 0x5C;
        public const int SEND_DATA = 0x60;
        public const int SET_CTRL = 0x64;
        public const int SET_PORT0 = 0x68;
        public const int SET_PORT1 = 0x6C;
        public const int SET_PORT2 = 0x70;
        public const int SET_PORT3 = 0x74;
        public const int DELAYSHORT = 0x78;
        public const int DELAYTIME = 0x7C;
        public const int DELAYDWORD = 0x80;
        public const int SETBYTETIME = 0x88;
        public const int SETVPWSTART = 0x08;
        // 最终要将SETVPWSTART转换成SETBYTETIME
        public const int SETWAITTIME = 0x8C;
        public const int SETLINKTIME = 0x90;
        public const int SETRECBBOUT = 0x94;
        public const int SETRECFROUT = 0x98;
        public const int SETVPWRECS = 0x14;
        // 最终要将SETVPWRECS转换成SETRECBBOUT
        public const int COPY_BYTE = 0x9C;
        public const int UPDATE_BYTE = 0xA0;
        public const int INC_BYTE = 0xA4;
        public const int DEC_BYTE = 0xA8;
        public const int ADD_BYTE = 0xAC;
        public const int SUB_BYTE = 0xB0;
        public const int INVERT_BYTE = 0xB4;
        public const int REC_FR = 0xE0;
        public const int REC_LEN_1 = 0xE1;
        public const int REC_LEN_2 = 0xE2;
        public const int REC_LEN_3 = 0xE3;
        public const int REC_LEN_4 = 0xE4;
        public const int REC_LEN_5 = 0xE5;
        public const int REC_LEN_6 = 0xE6;
        public const int REC_LEN_7 = 0xE7;
        public const int REC_LEN_8 = 0xE8;
        public const int REC_LEN_9 = 0xE9;
        public const int REC_LEN_10 = 0xEA;
        public const int REC_LEN_11 = 0xEB;
        public const int REC_LEN_12 = 0xEC;
        public const int REC_LEN_13 = 0xED;
        public const int REC_LEN_14 = 0xEE;
        public const int REC_LEN_15 = 0xEF;
        public const int RECEIVE = 0xF0;
        public const int RECV_ERR = 0xAA;
        // 接收错误
        public const int RECV_OK = 0x55;
        // 接收正确
        public const int BUSY = 0xBB;
        // 开始执行
        public const int READY = 0xDD;
        // 执行结束
        public const int ERROR = 0xEE;
        // 执行错误
        // RF多对一的设定接口,最多16个
        public const int RF_RESET = 0xD0;
        public const int RF_SETDTR_L = 0xD1;
        public const int RF_SETDTR_H = 0xD2;
        public const int RF_SET_BAUD = 0xD3;
        public const int RF_SET_ADDR = 0xD8;
        public const int COMMBOXID_ERR = 1;
        public const int DISCONNECT_COMM = 2;
        public const int DISCONNECT_COMMBOX = 3;
        public const int OTHER_ERROR = 4;
        // 錯誤標識
        public const int ERR_OPEN = 0x01;
        // OpenComm() 失敗
        public const int ERR_CHECK = 0x02;
        // CheckEcm() 失敗
        // 接頭標識定義
        public const int OBDII_16 = 0x00;
        public const int UNIVERSAL_3 = 0x01;
        public const int BENZ_38 = 0x02;
        public const int BMW_20 = 0x03;
        public const int AUDI_4 = 0x04;
        public const int FIAT_3 = 0x05;
        public const int CITROEN_2 = 0x06;
        public const int CHRYSLER_6 = 0x07;
        public const int TOYOTA_17R = 0x20;
        public const int TOYOTA_17F = 0x21;
        public const int HONDA_3 = 0x22;
        public const int MITSUBISHI = 0x23;
        public const int HYUNDAI = 0x23;
        public const int NISSAN = 0x24;
        public const int SUZUKI_3 = 0x25;
        public const int DAIHATSU_4 = 0x26;
        public const int ISUZU_3 = 0x27;
        public const int CANBUS_16 = 0x28;
        public const int GM_12 = 0x29;
        public const int KIA_20 = 0x30;
        // 常量定義
        public const int TRYTIMES = 3;
        // 通訊通道定義
        public const int SK0 = 0;
        public const int SK1 = 1;
        public const int SK2 = 2;
        public const int SK3 = 3;
        public const int SK4 = 4;
        public const int SK5 = 5;
        public const int SK6 = 6;
        public const int SK7 = 7;
        public const int SK_NO = 0xFF;
        public const int RK0 = 0;
        public const int RK1 = 1;
        public const int RK2 = 2;
        public const int RK3 = 3;
        public const int RK4 = 4;
        public const int RK5 = 5;
        public const int RK6 = 6;
        public const int RK7 = 7;
        public const int RK_NO = 0xFF;
        // 協議常量標誌定義
        public const int NO_PACK = 0x80;
        // 發送的命令不需要打包
        public const int UN_PACK = 0x08;
        // 接收到的數據解包處理
        public const int MFR_1 = 0x00;
        public const int MFR_2 = 0x02;
        public const int MFR_3 = 0x03;
        public const int MFR_4 = 0x04;
        public const int MFR_5 = 0x05;
        public const int MFR_6 = 0x06;
        public const int MFR_7 = 0x07;
        public const int MFR_N = 0x01;
        private static int[] password;

        static W80Commbox()
        {
            password = new int[] { 0x0C, 0x22, 0x17, 0x41, 0x57, 0x2D, 0x43, 0x17, 0x2D, 0x4D };
        }

        private int timeUnit;
        // 1/10000 seconds
        private int timeBaseDB;
        // standard time times
        private int timeExternDB;
        // expand time times
        private byte[] port;
        private byte[] buf;
        private int bufPos;
        private bool isLink;
        // is heartbeat block
        private int runFlag;
        //	private int startPos;
        private int boxVer;
        private bool isOpen;
        private SerialPort serialPort;
        private bool isDB20;
        private bool isDoNow;
        //	private Timer reqByteToByte;
        //	private Timer reqWaitTime;
        //	private Timer resByteToByte;
        private Timer resWaitTime;
        private int buffId;
        private byte[] sendCmdData;
        private byte[] getCmdDataCS;
        private byte[] doCmdData;
        private byte[] initCheckBoxBuff;
        Random initBoxRnd;
        private byte[] ctrlWord;
        private byte[] timeBuff;

        public W80Commbox()
        {
            timeUnit = 0;
            timeBaseDB = 0;
            timeExternDB = 0;
            port = new byte[MAXPORT_NUM];
            buf = new byte[MAXBUFF_LEN];
            bufPos = 0;
            isLink = false;
            runFlag = 0;
            //		startPos = 0;
            boxVer = 0;
            isOpen = false;
            serialPort = new SerialPort();
            isDB20 = false;
            isDoNow = true;
            //		reqByteToByte = Timer.fromMilliseconds(0);
            //		reqWaitTime = Timer.fromMilliseconds(0);
            //		resByteToByte = Timer.fromMilliseconds(0);
            resWaitTime = Timer.FromMilliseconds(0);
            buffId = 0;
            sendCmdData = new byte[256];
            getCmdDataCS = new byte[1];
            doCmdData = new byte[256];
            initCheckBoxBuff = new byte[32];
            initBoxRnd = new Random();
            ctrlWord = new byte[3];
            timeBuff = new byte[2];
        }

        public int BuffId
        {
            get { return buffId; }
            set { buffId = value; }
        }

        private void GetLinkTime(int type, Timer time)
        {
            switch (type)
            {
                case SETBYTETIME:
				//			reqByteToByte = time;
                    break;
                case SETWAITTIME:
				//			reqWaitTime = time;
                    break;
                case SETRECBBOUT:
				//			resByteToByte = time;
                    break;
                case SETRECFROUT:
                    resWaitTime = time;
                    break;
            }
        }

        private bool CheckSend()
        {
            try
            {
                int b = 0;
                serialPort.ReadTimeout = 200;

                b = serialPort.ReadByte();
                if (b != RECV_OK)
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool SendCmd(int cmd, byte[] buffer, int offset, int count)
        {
            int cs = cmd;
            int pos = 0;

            sendCmdData[pos++] = Utils.LoByte(cmd + runFlag);
            if (buffer != null)
            {
                for (int i = 0; i < count; i++)
                {
                    cs += buffer[offset + i];
                }
                Array.Copy(buffer, offset, sendCmdData, pos, count);
                pos += count;
            }

            sendCmdData[pos++] = Utils.LoByte(cs);

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    CheckIdle();
                    serialPort.Write(sendCmdData, 0, pos);
                }
                catch
                {
                    continue;
                }

                if (CheckSend())
                {
                    return true;
                }
            }

            return false;
        }

        private bool SendCmd(int cmd)
        {
            return SendCmd(cmd, null, 0, 0);
        }

        private int RecvBytes(byte[] buff, int offset, int count)
        {
            return ReadData(buff, offset, count, Timer.FromMilliseconds(500));
        }

        private bool GetCmdData(byte[] buff, int offset, int maxLen)
        {
            if (RecvBytes(buff, 0, 1) != 1)
                return false;
            if (RecvBytes(buff, 1, 1) != 1)
                return false;

            int len = buff[1];
            if (len > maxLen)
                len = maxLen;

            if (RecvBytes(buff, 0, len) != len)
                return false;

            getCmdDataCS[0] = 0;
            if (RecvBytes(getCmdDataCS, 0, 1) != 1)
                return false;

            return len > 0;
        }

        private bool DoCmd(int cmd, byte[] buffer, int offset, int count)
        {
            //		startPos = 0;
            int pos = 0;
            if (cmd != WR_DATA && cmd != SEND_DATA)
                cmd |= count; // 加上长度位
            if (isDoNow)
            {
                // 发送到BOX执行
                switch (cmd)
                {
                    case WR_DATA:
                        if (count == 0)
                            return false;
                        if (isLink)
                            doCmdData[pos++] = 0xFF; // 写链路保持
					else
                            doCmdData[pos++] = 0x00; // 写通讯命令
                        doCmdData[pos++] = Utils.LoByte(count);
                        Array.Copy(buffer, offset, doCmdData, pos, count);
                        pos += count;
                        return SendCmd(WR_DATA, doCmdData, 0, pos);
                    case SEND_DATA:
                        if (count == 0)
                            return false;
                        doCmdData[pos++] = 0; // 写入位置
                        doCmdData[pos++] = Utils.LoByte(count + 2); // 数据包长度
                        doCmdData[pos++] = Utils.LoByte(SEND_DATA); // 命令
                        doCmdData[pos++] = Utils.LoByte(count - 1); // 命令长度-1
                        Array.Copy(buffer, offset, doCmdData, pos, count);
                        pos += count;
                        if (!SendCmd(WR_DATA, doCmdData, 0, pos))
                            return false;
                        return SendCmd(DO_BAT_C);
                    default:
                        return SendCmd(cmd, buffer, offset, count);
                }
            }
            else
            {
                // 写命令到缓冲区
                buf[bufPos++] = Utils.LoByte(cmd);
                if (cmd == SEND_DATA)
                    buf[bufPos++] = Utils.LoByte(count - 1);
                //			startPos = pos;
                if (count > 0)
                {
                    Array.Copy(buffer, offset, buf, bufPos, count);
                    bufPos += count;
                }
                return true;
            }
        }

        private bool DoCmd(int cmd)
        {
            return DoCmd(cmd, null, 0, 0);
        }

        private bool DoSet(int cmd, byte[] buffer, int offset, int count)
        {
            bool result = false;
            try
            {
                result = DoCmd(cmd, buffer, offset, count);
                if (result && isDoNow)
                    CheckResult(Timer.FromMilliseconds(150));
            }
            catch
            {
                result = false;
            }
            return false;
        }

        private bool DoSet(int cmd)
        {
            return DoSet(cmd, null, 0, 0);
        }

        private bool DoSet(int cmd, params byte[] buffer)
        {
            return DoSet(cmd, buffer, 0, buffer.Length);
        }

        private bool InitBox()
        {
            isDoNow = true;

            int i;
            for (i = 1; i < 4; i++)
                initCheckBoxBuff[i] = Utils.LoByte(initBoxRnd.Next());

            int run = 0;
            for (i = 0; i < password.Length; i++)
                run += password[i] ^ (initCheckBoxBuff[i % 3 + 1]);

            run = run & 0xFF;
            if (run == 0)
                run = 0x55;

            if (!DoCmd(GET_CPU, initCheckBoxBuff, 1, 3))
                return false;

            if (!GetCmdData(initCheckBoxBuff, 0, 32))
                return false;

            runFlag = 0;
            timeUnit = 0;

            for (i = 0; i < 3; i++)
                timeUnit = timeUnit * 256 + (initCheckBoxBuff[i]);
            timeBaseDB = initCheckBoxBuff[i++];
            timeExternDB = initCheckBoxBuff[i++];

            for (i = 0; i < MAXPORT_NUM; i++)
                port[i] = 0xFF;
            bufPos = 0;
            isDB20 = false;

            return true;
        }

        private bool CheckBox()
        {
            if (!DoCmd(GET_BOXID))
            {
                return false;
            }
            if (!GetCmdData(initCheckBoxBuff, 0, 32))
            {
                return false;
            }
            boxVer = (initCheckBoxBuff[10] << 8) | initCheckBoxBuff[11];
            return true;
        }

        private bool UpdateBuff(int type, int addr, int data)
        {
            byte[] buff = new byte[3];
            int len = 0;
            buff[0] = Utils.LoByte(addr);
            buff[1] = Utils.LoByte(data);

            switch (type)
            {
                case INC_BYTE:
                case DEC_BYTE:
                case INVERT_BYTE:
                    len = 1;
                    break;
                case UPDATE_BYTE:
                case ADD_BYTE:
                case SUB_BYTE:
                    len = 2;
                    break;
                case COPY_BYTE:
                    len = 3;
                    break;
            }

            return DoSet(type, buff, 0, len);
        }

        private bool CopyBuff(int dest, int src, int len)
        {
            byte[] buff = new byte[3];
            buff[0] = Utils.LoByte(dest);
            buff[1] = Utils.LoByte(src);
            buff[2] = Utils.LoByte(len);
            return DoSet(COPY_BYTE, buff);
        }

        private bool Reset()
        {
            try
            {
                StopNow(true);
            }
            catch
            {
            }
            Clear();
            for (int i = 0; i < MAXPORT_NUM; i++)
                port[i] = 0xFF;
            return DoCmd(RESET);
        }

        public void Clear()
        {
            try
            {
                serialPort.DiscardInBuffer();
                serialPort.DiscardOutBuffer();
            }
            catch
            {
            }
        }

        public int BoxVer
        {
            get { return boxVer; }
        }

        public void CheckIdle()
        {
            int avail;
            try
            {
                avail = serialPort.BytesToRead;
                if (avail > 20)
                {
                    Clear();
                    return;
                }

                int b = READY;
                serialPort.ReadTimeout = 200;
                while ((avail = serialPort.BytesToRead) != 0)
                {
                    b = serialPort.ReadByte();
                }

                if (b == READY || b == ERROR)
                    return;
            }
            catch
            {
                throw new CommboxException("CheckIdle fail!");
            }
        }

        public void CheckResult(Timer time)
        {
            try
            {
                serialPort.ReadTimeout = (int)time.Milliseconds;
                int rb = 0;
                if ((rb = serialPort.ReadByte()) != -1)
                {
                    if (rb == READY || rb == ERROR)
                    {
                        Clear();
                        return;
                    }
                }
            }
            catch
            {
            }

            throw new CommboxException("CheckResult fail!");
        }

        public int ReadData(byte[] buffer, int offset, int count, Timer time)
        {
            try
            {
                serialPort.ReadTimeout = (int)time.Milliseconds;

                int len = serialPort.Read(buffer, offset, count);
                if (len < count)
                {
                    int avail = serialPort.BytesToRead;
                    if (avail > 0)
                    {
                        if (avail <= (count - len))
                        {
                            len += serialPort.Read(buffer, offset + len, avail);
                        }
                        else
                        {
                            len += serialPort.Read(buffer, offset + len, count - len);
                        }
                    }
                }
                return len;
            }
            catch
            {
            }

            return 0;
        }

        public void StopNow(bool isStopExecute)
        {
            int cmd = isStopExecute ? STOP_EXECUTE : STOP_REC;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    serialPort.Write(Utils.LoByte(cmd));
                    if (CheckSend())
                    {
                        if (isStopExecute)
                        {
                            CheckResult(Timer.FromMilliseconds(200));
                        }
                    }
                }
                catch
                {
                }
            }

            throw new CommboxException("StopNow fail!");
        }

        public void NewBatch()
        {
            bufPos = 0;
            isLink = (BuffId == LINKBLOCK ? true : false);
            isDoNow = false;
        }

        public void DelBatch()
        {
            isDoNow = true;
            bufPos = 0;
        }

        public void EndBatch()
        {
            int i = 0;
            isDoNow = true;
            buf[bufPos++] = 0; // 命令块以0x00标记结束
            if (isLink)
            {
                // 修改UpdateBuff使用到的地址
                while (buf[i] != 0)
                {
                    int tmp = MAXBUFF_LEN - bufPos;
                    switch (buf[i] & 0xFC)
                    {
                        case COPY_BYTE:
                            buf[i + 3] = Utils.LoByte(buf[i + 3] + tmp);
                            goto case SUB_BYTE;
                        case SUB_BYTE:
                            buf[i + 2] = Utils.LoByte(buf[i + 2] + tmp);
                            goto case UPDATE_BYTE;
                        case UPDATE_BYTE:
                        case INVERT_BYTE:
                        case ADD_BYTE:
                        case DEC_BYTE:
                        case INC_BYTE:
                            buf[i + 1] = Utils.LoByte(buf[i + 1] + tmp);
                            break;
                    }

                    if (buf[i] == SEND_DATA)
                        i += 1 + buf[i + 1] + 1 + 1;
                    else if (buf[i] >= REC_LEN_1
                    && buf[i] <= REC_LEN_15)
                        i += 1; // 特殊
                }
            }

            if (!DoCmd(WR_DATA, buf, 0, bufPos))
                throw new CommboxException("EndBatch fail!");
        }

        public void SetLineLevel(int valueLow, int valueHigh)
        {
            // 设定端口1
            port[1] &= (byte)~valueLow;
            port[1] |= (byte)valueHigh;
            if (!DoSet(SET_PORT1, port[1]))
                throw new CommboxException("SetLineLevel fail!");
        }

        public void SetCommCtrl(int valueOpen, int valueClose)
        {
            // 设定端口2
            port[2] &= (byte)~valueOpen;
            port[2] |= (byte)valueClose;
            if (!DoSet(SET_PORT2, port[2]))
                throw new CommboxException("SetCommCtrl fail!");
        }

        public void SetCommLine(int sendLine, int recvLine)
        {
            // 设定端口0
            if (sendLine > 7)
                sendLine = 0x0F;
            if (recvLine > 7)
                recvLine = 0x0F;

            port[0] = (byte)(sendLine | (recvLine << 4));
            if (!DoSet(SET_PORT0, port[0]))
                throw new CommboxException("SetCommLine fail!");
        }

        public void TurnOverOneByOne()
        {
            // 将原有的接受一个发送一个的标志翻转
            if (!DoSet(SET_ONEBYONE))
                throw new CommboxException("TurnOverOneByOne fail!");
        }

        public void KeepLink(bool isRunLink)
        {
            if (!DoSet(isRunLink ? RUN_LINK : STOP_LINK))
                throw new CommboxException("KeepLink fail!");
        }

        public void SetCommLink(int ctrlWord1, int ctrlWord2, int ctrlWord3)
        {
            int modeControl = ctrlWord1 & 0xE0;
            int length = 3;
            ctrlWord[0] = Utils.LoByte(ctrlWord1);

            if ((ctrlWord1 & 0x04) != 0)
                isDB20 = true;
            else
                isDB20 = false;

            if (modeControl == SET_VPW || modeControl == SET_PWM)
            {
                if (!DoSet(SET_CTRL, ctrlWord[0]))
                    throw new CommboxException("SetCommLink fail!");
            }

            ctrlWord[1] = Utils.LoByte(ctrlWord2);
            ctrlWord[2] = Utils.LoByte(ctrlWord3);
            if (ctrlWord3 == 0)
            {
                length--;
                if (ctrlWord2 == 0)
                {
                    length--;
                }
            }

            if (modeControl == EXRS_232 && length < 2)
                throw new CommboxException("SetCommLink fail!");

            if (!DoSet(SET_CTRL, ctrlWord, 0, length))
                throw new CommboxException("SetCommLink fail!");
        }

        public void SetCommBaud(double baud)
        {
            double instructNum = 1000000000000.0 / (timeUnit * baud);
            if (isDB20)
                instructNum /= 20;
            instructNum += 0.5;
            if (instructNum > 65535 || instructNum < 10)
                throw new CommboxException("SetCommBaud fail!");
            timeBuff[0] = Utils.HiByte((long)instructNum);
            timeBuff[1] = Utils.LoByte((long)instructNum);

            if (timeBuff[0] == 0)
            {
                if (!DoSet(SET_BAUD, timeBuff[1]))
                    throw new CommboxException("SetCommBaud fail!");
            }
            else
            {
                if (!DoSet(SET_BAUD, timeBuff))
                    throw new CommboxException("SetCommBaud fail!");
            }
        }

        public void SetCommTime(int type, Timer time)
        {
            GetLinkTime(type, time);

            long microTime = time.Microseconds;
            if (type == SETVPWSTART || type == SETVPWRECS)
            {
                if (type == SETVPWRECS)
                    microTime = (microTime * 2) / 3;
                type = type + (SETBYTETIME & 0xF0);
                microTime = (long)((microTime * 1000000.0) / timeUnit);
            }
            else
            {
                microTime = (long)((microTime * 1000000.0) / (timeBaseDB * timeUnit));
            }
            timeBuff[0] = Utils.HiByte(microTime);
            timeBuff[1] = Utils.LoByte(microTime);

            if (timeBuff[0] == 0)
            {
                if (!DoSet(type, timeBuff[1]))
                    throw new CommboxException("SetCommTime fail!");
            }
            else
            {
                if (!DoSet(type, timeBuff))
                    throw new CommboxException("SetCommTime fail!");
            }
        }

        public void RunReceive(int type)
        {
            if (type == GET_PORT1)
                isDB20 = false;
            if (!DoCmd(type))
                throw new CommboxException("RunReceive fail!");
        }

        public void CommboxDelay(Timer time)
        {
            int delayWord = DELAYSHORT;
            long microTime = (long)(time.Microseconds / (timeUnit / 1000000.0));

            if (microTime == 0)
                throw new CommboxException("CommboxDelay fail!");
            if (microTime > 65535)
            {
                microTime = microTime / timeBaseDB;
                if (microTime > 65535)
                {
                    microTime = (microTime * timeBaseDB) / timeExternDB;
                    if (microTime > 65535)
                        throw new CommboxException("CommboxDelay fail!");
                    delayWord = DELAYDWORD;
                }
                else
                {
                    delayWord = DELAYTIME;
                }
            }

            timeBuff[0] = Utils.HiByte(microTime);
            timeBuff[1] = Utils.LoByte(microTime);

            if (timeBuff[0] == 0)
            {
                if (!DoSet(delayWord, timeBuff[1]))
                    throw new CommboxException("CommboxDelay fail!");
            }
            else
            {
                if (!DoSet(delayWord, timeBuff))
                    throw new CommboxException("CommboxDelay fail!");
            }
        }

        public void SendOutData(params byte[] bs)
        {
            SendOutData(bs, 0, bs.Length);
        }

        public void SendOutData(byte[] buff, int offset, int count)
        {
            if (!DoSet(SEND_DATA, buff, offset, count))
                throw new CommboxException("SendOutData fail!");
        }

        public void RunBatch(bool repeat)
        {
            int cmd;
            if (buffId == LINKBLOCK)
                cmd = repeat ? DO_BAT_LN : DO_BAT_L;
            else
                cmd = repeat ? DO_BAT_CN : DO_BAT_C;
            if (!DoCmd(cmd))
                throw new CommboxException("RunBatch fail!");
        }

        public int ReadBytes(byte[] buff, int offset, int count)
        {
            return ReadData(buff, offset, count, resWaitTime);
        }

        public void Connect()
        {
            string[] infos = SerialPort.GetPortNames();

            foreach (string info in infos)
            {
                try
                {
                    serialPort.Close();
                    serialPort.PortName = info;
                    serialPort.BaudRate = 115200;
                    serialPort.Parity = Parity.None;
                    serialPort.DataBits = 8;
                    serialPort.StopBits = StopBits.Two;
                    serialPort.ReadTimeout = 500;

                    serialPort.Open();

                    serialPort.DtrEnable = false;
                    Thread.Sleep(50);
                    serialPort.DtrEnable = true;
                    Thread.Sleep(50);

                    for (int i = 0; i < 3; i++)
                    {
                        if (InitBox() && CheckBox())
                        {
                            Clear();
                            IsOpen = true;
                            return;
                        }
                    }
                }
                catch
                {
                }
            }

            throw new CommboxException("Connect fail!");
        }

        public void Disconnect()
        {
            if (IsOpen)
            {
                Reset();
                serialPort.Close();
                IsOpen = false;
            }
        }

        public bool IsOpen
        {
            get { return isOpen; }
            private set { isOpen = value; }
        }
    }
}

