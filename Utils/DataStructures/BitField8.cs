using System;

namespace VideoCompressorGUI.Utils.DataStructures
{
    [System.Serializable]
    public struct Bitfield8
    {
        public byte Data { get; set; }
        private static readonly byte SIZE = 8;

        public Bitfield8(byte data)
        {
            this.Data = data;
        }

        public void ForEach(Action<bool> action)
        {
            for (int i = 0; i < SIZE; i++)
                action.Invoke(GetAtFast(i));
        }

        public void ForEachIndex(Action<bool, int> action)
        {
            for (int i = 0; i < SIZE; i++)
                action.Invoke(GetAtFast(i), i);
        }

        private bool GetAtFast(int index)
            => (Data & 1 << index) != 0;

        public static Bitfield8 operator <<(Bitfield8 mask, int value)
        {
            mask.Data = 1;
            mask.Data <<= value;
            return mask;
        }

        public static explicit operator Bitfield8(int value) => new((byte) value);
        public static explicit operator Bitfield8(byte value) => new(value);
        
        public static bool operator ==(Bitfield8 mask1, Bitfield8 mask2)
            => mask1.Data == mask2.Data;

        public static bool operator !=(Bitfield8 mask1, Bitfield8 mask2)
            => mask1.Data != mask2.Data;

        public static bool operator ==(Bitfield8 mask1, uint mask2)
            => mask1.Data == mask2;

        public static bool operator !=(Bitfield8 mask1, uint mask2)
            => mask1.Data != mask2;

        public static bool operator ==(uint mask1, Bitfield8 mask2)
            => mask1 == mask2.Data;

        public static bool operator !=(uint mask1, Bitfield8 mask2)
            => !(mask2 == mask1);

        /// <summary>
        /// Determines, if the bit at the given index if flipped. Leading bit is most right 
        /// </summary>
        public bool this[int index]
        {
            get
            {
                if (index < 0 || index >= SIZE)
                    throw new IndexOutOfRangeException($"You need an index between 0 and {SIZE}");

                return (Data & 1 << index) != 0;
            }
            set
            {
                if (index < 0 || index >= SIZE)
                    throw new IndexOutOfRangeException($"You need an index between 0 and {SIZE}");

                Data = value ? (byte)(Data | (byte)(1 << index)) : (byte)(Data & (byte)~(1 << index));
            }
        }

        public byte ToByte()
            => Data;

        public override string ToString()
            => Data.ToString();

        public override bool Equals(object obj)
        {
            return obj is Bitfield8 masker && Data == masker.Data;
        }

        public override int GetHashCode()
        {
            return Data.GetHashCode();
        }

        public bool AnyTrue()
        {
            for (int i = 0; i < SIZE; i++)
            {
                if (GetAtFast(i))
                    return true;
            }

            return false;
        }
    }
}