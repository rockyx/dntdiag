using System;

namespace DNT.Diag.Attribute
{
	public class Attribute
	{
		// Common
		private byte[] heartbeat;

		public byte[] Heartbeat {
			get {
				return heartbeat;
			}
			set {
				heartbeat = value;
			}
		}

		// K Line
		private KLineParity klineParity;

		public KLineParity KLineParity {
			get {
				return klineParity;
			}
			set {
				klineParity = value;
			}
		}

		private bool klineL;

		public bool KLineL {
			get {
				return klineL;
			}
			set {
				klineL = value;
			}
		}

		private int klineAddrCode;

		public int KLineAddrCode {
			get {
				return klineAddrCode;
			}
			set {
				klineAddrCode = value;
			}
		}

		private int klineTargetAddress;

		public int KLineTargetAddress {
			get {
				return klineTargetAddress;
			}
			set {
				klineTargetAddress = value;
			}
		}

		private int klineSourceAddress;

		public int KLineSourceAddress {
			get {
				return klineSourceAddress;
			}
			set {
				klineSourceAddress = value;
			}
		}

		private int klineBaudRate;

		public int KLineBaudRate {
			get {
				return klineBaudRate;
			}
			set {
				klineBaudRate = value;
			}
		}

		private int klineComLine;

		public int KLineComLine {
			get {
				return klineComLine;
			}
			set {
				klineComLine = value;
			}
		}

		// KWP 2000
		private KWP2KStartType kwp2KStartType;

		public KWP2KStartType KWP2KStartType {
			get {
				return kwp2KStartType;
			}
			set {
				kwp2KStartType = value;
			}
		}

		private KWP2KMode kwp2kHeartbeatMode;

		public KWP2KMode KWP2kHeartbeatMode {
			get {
				return kwp2kHeartbeatMode;
			}
			set {
				kwp2kHeartbeatMode = value;
			}
		}

		private KWP2KMode kwp2kMsgMode;

		public KWP2KMode KWP2kMsgMode {
			get {
				return kwp2kMsgMode;
			}
			set {
				kwp2kMsgMode = value;
			}
		}

		private KWP2KMode kwp2kCurrentMode;

		public KWP2KMode KWP2kCurrentMode {
			get {
				return kwp2kCurrentMode;
			}
			set {
				kwp2kCurrentMode = value;
			}
		}

		private byte[] kwp2kFastCmd;

		public byte[] KWP2kFastCmd {
			get {
				return kwp2kFastCmd;
			}
			set {
				kwp2kFastCmd = value;
			}
		}

		// ISO9141
		private int isoHeader;

		public int ISOHeader {
			get {
				return isoHeader;
			}
			set {
				isoHeader = value;
			}
		}

		// Canbus
		private int canbusIdForEcuRecv;

		public int CanbusIdForEcuRecv {
			get {
				return canbusIdForEcuRecv;
			}
			set {
				canbusIdForEcuRecv = value;
			}
		}

		private CanBaudRate canbusBaudRate;

		public CanBaudRate CanbusBaudRate {
			get {
				return canbusBaudRate;
			}
			set {
				canbusBaudRate = value;
			}
		}

		private CanIDMode canbusIDMode;

		public CanIDMode CanbusIDMode {
			get {
				return canbusIDMode;
			}
			set {
				canbusIDMode = value;
			}
		}

		private CanFilterMask canbusFilterMask;

		public CanFilterMask CanbusFilterMask {
			get {
				return canbusFilterMask;
			}
			set {
				canbusFilterMask = value;
			}
		}

		private int canbusHighPin;

		public int CanbusHighPin {
			get {
				return canbusHighPin;
			}
			set {
				canbusHighPin = value;
			}
		}

		private int canbusLowPin;

		public int CanbusLowPin {
			get {
				return canbusLowPin;
			}
			set {
				canbusLowPin = value;
			}
		}

		private int[] canbusIDRecvFilters;

		public int[] CanbusIDRecvFilters {
			get {
				return canbusIDRecvFilters;
			}
			set {
				canbusIDRecvFilters = value;
			}
		}

		private byte[] canbusFlowControl;

		public byte[] CanbusFlowControl {
			get {
				return canbusFlowControl;
			}
			set {
				canbusFlowControl = value;
			}
		}
	}
}

