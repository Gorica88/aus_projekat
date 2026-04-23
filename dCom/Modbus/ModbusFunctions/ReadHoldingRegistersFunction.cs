using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read holding registers functions/requests.
    /// </summary>
    public class ReadHoldingRegistersFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadHoldingRegistersFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadHoldingRegistersFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusReadCommandParameters readParams = (ModbusReadCommandParameters)CommandParameters;

            byte[] request = new byte[12];
            int offset = 0;

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)readParams.TransactionId)), 0, request, offset, 2);
            offset += 2;

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)readParams.ProtocolId)), 0, request, offset, 2);
            offset += 2;

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)readParams.Length)), 0, request, offset, 2);
            offset += 2;

            request[offset++] = readParams.UnitId;
            request[offset++] = readParams.FunctionCode;

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)readParams.StartAddress)), 0, request, offset, 2);
            offset += 2;

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)readParams.Quantity)), 0, request, offset, 2);

            return request;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            Dictionary<Tuple<PointType, ushort>, ushort> result = new Dictionary<Tuple<PointType, ushort>, ushort>();
            ModbusReadCommandParameters readParams = (ModbusReadCommandParameters)CommandParameters;

            int byteCount = response[8];

            for (int i = 0; i < byteCount / 2; i++)
            {
                ushort value = (ushort)((response[9 + i * 2] << 8) | response[10 + i * 2]);
                result.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, (ushort)(readParams.StartAddress + i)), value);
            }

            return result;
        }
    }
}