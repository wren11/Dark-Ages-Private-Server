using System;

namespace Proxy.Networking
{
    public sealed class CryptoProvider
    {
        private CryptoParams _params;
        private byte[] Seed { get; set; } = new byte[256];

        public CryptoParams CryptoParams
        {
            get => _params;
            set
            {
                _params = value;
                GenerateSeed(CryptoParams.SeedByte);
            }
        }

        public CryptoProvider()
        {
            _params = new CryptoParams();
            GenerateSeed(CryptoParams.SeedByte);
        }

        public CryptoProvider(CryptoParams eParams)
        {
            _params = eParams;
            GenerateSeed(CryptoParams.SeedByte);
        }

        public void Transform(Packet data)
        {
            if (data == null)
                return;

            var numArray = new byte[data.Data.Length - 2];
            Array.Copy(data.Data, 2, numArray, 0, data.Data.Length - 2);

            for (var index = 0; index < numArray.Length; ++index)
            {
                numArray[index] = (byte) (numArray[index] ^
                                          (uint) CryptoParams.PrivateKey[index % CryptoParams.PrivateKey.Length] ^
                                          Seed[data.Ordinal] ^
                                          Seed[index / CryptoParams.PrivateKey.Length % Seed.Length]);
                if (index / CryptoParams.PrivateKey.Length == data.Ordinal)
                    numArray[index] = (byte) (numArray[index] ^ (uint) Seed[data.Ordinal]);
            }

            Array.Copy(numArray, 0, data.Data, 2, numArray.Length);
        }

        public void GenerateSeed(byte seedValue)
        {
            var numArray = new byte[256];

            switch (seedValue)
            {
                case 1:
                    for (int index = 0; index < numArray.Length; ++index)
                        numArray[index] = (byte) (128 + (index + 1) / 2 * (index % 2 == 0 ? 1 : -1));
                    break;
                case 2:
                    for (int index = 0; index < numArray.Length; ++index)
                        numArray[index] = (byte) (byte.MaxValue - index);
                    break;
                case 3:
                    for (int index = 0; index < numArray.Length; ++index)
                        numArray[index] = (byte) (byte.MaxValue * ((index + 1) % 2) +
                                                  (index + 1) / 2 * ((index + 1) % 2 == 0 ? 1 : -1));
                    break;
                case 4:
                    for (int index = 0; index < numArray.Length; ++index)
                        numArray[index] = (byte) (index / 16 * (index / 16));
                    break;
                case 5:
                    for (int index = 0; index < numArray.Length; ++index)
                        numArray[index] = (byte) (2 * index % 256);
                    break;
                case 6:
                    for (int index = 0; index < numArray.Length; ++index)
                        numArray[index] = (byte) (byte.MaxValue - 2 * index % 256);
                    break;
                case 7:
                    for (int index = 0; index < numArray.Length; ++index)
                        numArray[index] = index >= 128
                            ? (byte) (2 * index % 256)
                            : (byte) (byte.MaxValue - 2 * index % 256);
                    break;
                case 8:
                    for (int index = 0; index < numArray.Length; ++index)
                        numArray[index] = index >= 128
                            ? (byte) (byte.MaxValue - 2 * index % 256)
                            : (byte) (2 * index % 256);
                    break;
                case 9:
                    for (int index = 0; index < numArray.Length; ++index)
                    {
                        if (index < 128)
                        {
                            int num1 = 30 * ((index - 1) / 8 + 1);
                            int num2 = (index - 1) / 8 * ((index - 1) / 8);
                            numArray[index] = index <= 0 ? byte.MaxValue : (byte) (num1 - num2);
                        }
                        else
                        {
                            int num1 = 30 * (16 - index % 128 / 8);
                            int num2 = (15 - index % 128 / 8) * (15 - index % 128 / 8);
                            numArray[index] = (byte) (num1 - num2);
                        }
                    }

                    break;
                default:
                    for (int index = 0; index < numArray.Length; ++index)
                        numArray[index] = (byte) index;
                    break;
            }

            Seed = numArray;
        }

        public void DialogTransform(Packet Data)
        {
            for (var index = 5; index < Data.Data.Length - 1; ++index)
            {
                Data.Data[index] = (byte) (Data.Data[index] ^ (uint) (Data.Data[4] - 75 + (index - 5)));
            }
        }

        private static byte P(Packet value)
        {
            return (byte)(value.Data[0x1] ^ (byte)(value.Data[0x0] - 0x2D));
        }
    }
}
