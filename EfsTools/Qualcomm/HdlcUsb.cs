using System;
using System.Linq;
using EfsTools.Utils;
using LibUsbDotNet;
using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;
using LibUsbDotNet.Info;

namespace EfsTools.Qualcomm
{
    internal class HdlcUsb : IDisposable
    {
        private const int QualcommVid = 0x05C6;
        private const byte VendorSpecificClass = 0xFF;
        private const int DefaultTimeout = 15000;

        private readonly int _vid;
        private readonly int _pid;
        private readonly int _timeout;
        private readonly bool _sendControlChar;

        private readonly byte[] _readBuffer = new byte[1048576];

        private UsbContext _context;
        private IUsbDevice _device;
        private UsbEndpointReader _reader;
        private UsbEndpointWriter _writer;
        private int _interfaceId;

        public HdlcUsb(int vid, int pid, int timeout, bool sendControlChar)
        {
            _vid = vid;
            _pid = pid;
            _timeout = timeout;
            _sendControlChar = sendControlChar;
        }

        public bool IsOpen => _device != null && _device.IsOpen;

        public string DeviceName =>
            _device != null ? $"USB {_device.VendorId:x4}:{_device.ProductId:x4}" : "USB (not connected)";

        public bool SendControlChar => _sendControlChar;

        public void Dispose()
        {
            Close();
            _context?.Dispose();
            _context = null;
        }

        public void Open()
        {
            _context = new UsbContext();

            _device = _pid > 0
                ? _context.Find(new UsbDeviceFinder { Vid = _vid, Pid = _pid })
                : FindQualcommDiagDevice(_context, _vid);

            if (_device == null)
                throw new InvalidOperationException(
                    _pid > 0
                        ? $"USB device {_vid:x4}:{_pid:x4} not found"
                        : $"No Qualcomm DIAG USB device found (VID 0x{_vid:X4})");

            _device.Open();

            FindDiagEndpoints(_device, out var readEp, out var writeEp, out _interfaceId);

            if (_device is UsbDevice concreteDevice &&
                concreteDevice.SupportsDetachKernelDriver() &&
                concreteDevice.IsKernelDriverActive(_interfaceId))
            {
                concreteDevice.DetachKernelDriver(_interfaceId);
            }

            _device.ClaimInterface(_interfaceId);

            _reader = _device.OpenEndpointReader((ReadEndpointID)readEp, _readBuffer.Length);
            _writer = _device.OpenEndpointWriter((WriteEndpointID)writeEp);
        }

        public void Close()
        {
            if (_device != null && _device.IsOpen)
            {
                _device.ReleaseInterface(_interfaceId);
                _device.Close();
            }

            _device?.Dispose();
            _device = null;
            _reader = null;
            _writer = null;
        }

        public void Write(byte[] data)
        {
            var encoded = HdlcEncoder.Encode(data, _sendControlChar);
            var error = _writer.Write(encoded, _timeout, out _);
            if (error != Error.Success)
                throw new InvalidOperationException($"USB bulk write failed: {error}");
        }

        public byte[] Read()
        {
            var error = _reader.Read(_readBuffer, _timeout, out var bytesRead);
            if (error != Error.Success)
                throw new InvalidOperationException($"USB bulk read failed: {error}");
            return HdlcEncoder.Decode(_readBuffer, bytesRead);
        }

        private static IUsbDevice FindQualcommDiagDevice(UsbContext context, int vid)
        {
            foreach (var device in context.List())
            {
                if (device.VendorId != vid)
                    continue;

                if (HasDiagInterface(device))
                    return device;
            }

            return null;
        }

        private static bool HasDiagInterface(IUsbDevice device)
        {
            try
            {
                if (!device.TryGetConfigDescriptor(0, out var config))
                    return false;

                return config.Interfaces.Any(iface =>
                    iface.Class == ClassCode.VendorSpec &&
                    iface.SubClass == VendorSpecificClass &&
                    iface.Endpoints.Count >= 2);
            }
            catch
            {
                return false;
            }
        }

        private static void FindDiagEndpoints(IUsbDevice device, out byte readEp, out byte writeEp, out int interfaceId)
        {
            foreach (var config in device.Configs)
            {
                foreach (var iface in config.Interfaces)
                {
                    if (iface.Class != ClassCode.VendorSpec)
                        continue;

                    byte foundRead = 0, foundWrite = 0;
                    foreach (var ep in iface.Endpoints)
                    {
                        var direction = (EndpointDirection)(ep.EndpointAddress & 0x80);
                        var transferType = (TransferType)(ep.Attributes & 0x03);
                        if (transferType != TransferType.Bulk)
                            continue;

                        if (direction == EndpointDirection.In && foundRead == 0)
                            foundRead = ep.EndpointAddress;
                        else if (direction == EndpointDirection.Out && foundWrite == 0)
                            foundWrite = ep.EndpointAddress;
                    }

                    if (foundRead != 0 && foundWrite != 0)
                    {
                        readEp = foundRead;
                        writeEp = foundWrite;
                        interfaceId = iface.Number;
                        return;
                    }
                }
            }

            throw new InvalidOperationException("No DIAG bulk endpoints found on USB device");
        }
    }
}
