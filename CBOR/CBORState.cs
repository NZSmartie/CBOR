using System.Linq;

namespace CBOR
{
    internal class CBORState
    {
        private CBORMajorType _majorType;
        internal CBORMajorType MajorType
        {
            get { return _majorType; }
            set
            {
                _majorType = value;
                IsCollection = CBORReader.Collectiontypes.Contains(value);
            }
        }

        internal int SimpleType;

        internal int Tag;

        internal bool IsCollection { get; private set; }

        internal bool IsIndefinite { get => Length < 0; set { if (value) Length = -1; } }

        internal int Length = 0;
    }
}
