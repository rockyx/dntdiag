using System;

namespace DNT.Diag.Data
{
	public class LiveDataBuffer
	{
		private byte[] buff;
		private int length;

		public LiveDataBuffer ()
		{
			buff = new byte[32];
			length = 0;
		}

		public byte[] Buff
		{
			get {
				lock (this) {
					return buff;
				}
			}
		}

		public int Length
		{
			get {
				lock (this) {
					return length;
				}
			}
			private set { length = value; }
		}

		public void CopyTo(byte[] buff, int offset, int length)
		{
			lock (this) {
				if (this.buff.Length < length) {
					this.buff = new byte[length];
				}
				Array.Copy (buff, offset, this.buff, 0, length);
				Length = length;
			}
		}

		public byte this[int index]
		{
			get {
				lock (this) {
					return buff [index];
				}
			}
		}
	}
}

