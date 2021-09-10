﻿//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace nanoFramework.UI
{
    /// <summary>
    /// Display orientation. No all display drivers support every orientation.
    /// </summary>
    public enum DisplayOrientation : int
    {
        /// <summary>
        ///     Portrait
        /// </summary>
        PORTRAIT,
        /// <summary>
        ///     Portrait 180
        /// </summary>
        PORTRAIT180,
        /// <summary>
        ///     Landscape
        /// </summary>
        LANDSCAPE,
        /// <summary>
        ///     Landscape 180
        /// </summary>
        LANDSCAPE180
    };

    /// <summary>
    /// Display Control.  
    /// </summary>
    public static class DisplayControl
    {
        static private Bitmap _fullScreen = null;

        /// <summary>
        /// The maximum buffer size for Bitmap in bytes.
        /// </summary>
        public static uint MaximumBufferSize { get; internal set; }

        /// <summary>
        /// Initializes the screen for use with Spi configuration.
        /// </summary>
        /// <param name="spi">Spi configuration.</param>
        /// <param name="x">The x offset the screen start in the driver.</param>
        /// <param name="y">The y offset the screen start in the driver.</param>
        /// <param name="width">The width of the screen.</param>
        /// <param name="height">The height of the screen.</param>
        /// <param name="bufferSize">The desired buffer size allocation, 0 for default.</param>
        /// <remarks>You may have to configure the pins properly for the Spi configuration to be valid before initializing your screen.</remarks>
        /// <returns>The maximum buffer size possible allocation in bytes.</returns>
        public static uint Initialize(SpiConfiguration spi, ushort x, ushort y, ushort width, ushort height, uint bufferSize = 0)
        {
            Debug.WriteLine($"spibus={spi.SpiBus},cs={spi.ChipSelect},dc={spi.DataCommand},rst={spi.Reset},bl={spi.BackLight}");
            MaximumBufferSize = NativeInitSpi(spi.SpiBus, spi.ChipSelect, spi.DataCommand, spi.Reset, spi.BackLight, x, y, width, height, bufferSize);
            return MaximumBufferSize;
        }

        /// <summary>
        /// Initializes the screen to use with I2C configuration.
        /// </summary>
        /// <param name="i2c"></param>
        /// <param name="x">The x offset the screen start in the driver.</param>
        /// <param name="y">The y offset the screen start in the driver.</param>
        /// <param name="width">The width of the screen.</param>
        /// <param name="height">The height of the screen.</param>
        /// <param name="bufferSize">The desired buffer size allocation, 0 for default.</param>
        /// <remarks>You may have to configure the pins properly for the I2C configuration to be valid before initializing your screen.</remarks>
        /// <returns>The maximum buffer size possible allocation in bytes.</returns>
        public static uint Initialize(I2cConfiguration i2c, ushort x, ushort y, ushort width, ushort height, uint bufferSize = 0)
        {
            MaximumBufferSize = NativeInitI2c(i2c.I2cBus, i2c.Address, i2c.FastMode, x, y, width, height, bufferSize);
            return MaximumBufferSize;
        }

        /// <summary>
        /// Returns a bitmap the size of the current display. 
        /// </summary>
        public static Bitmap FullScreen
        {
            get
            {
                if (!IsFullScreenBufferAvailable)
                {
                    throw new System.Exception("Not enough memory");
                }

                if (_fullScreen == null)
                {
                    _fullScreen = new Bitmap(ScreenWidth, ScreenHeight);
                }
                return _fullScreen;
            }
        }

        public static bool IsFullScreenBufferAvailable => ScreenWidth * ScreenHeight * BitsPerPixel / 8 < MaximumBufferSize;

        /// <summary>
        /// The screens number of pixels for the longer side.
        /// </summary>
        extern static public int LongerSide
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        /// <summary>
        /// The screens number of pixels for the shorter side.
        /// </summary>
        extern static public int ShorterSide
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        /// <summary>
        /// The displays number of pixel for the width based on the orientation.
        /// </summary>
        extern static public int ScreenWidth
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        /// <summary>
        /// The displays number of pixel for the height based on the orientation.
        /// </summary>
        extern static public int ScreenHeight
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        /// <summary>
        /// Currently 16 bits in RBG565 format. ( There is some 1 bit code available but untested )
        /// </summary>
        extern static public int BitsPerPixel
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        /// <summary>
        /// Return the current display orientation landscape, portrait.
        /// </summary>
        extern static public DisplayOrientation Orientation
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        /// <summary>
        /// Change the orientation of the display.
        /// </summary>
        /// <remarks>
        /// When the orientation is changed the display canvas is disposed and recreated with the new dimensions
        /// when DisplayControl.FullScreen is next called.
        /// </remarks>
        /// <param name="Orientation">New Orientation</param>
        /// <returns>True if the orientation was supported and changed.</returns>
        static public bool ChangeOrientation(DisplayOrientation Orientation)
        {
            bool result = NativeChangeOrientation(Orientation);
            // if change happened then destroy bitmap as it needs to be recreated with new dimensions.
            if (result && _fullScreen != null)
            {
                _fullScreen.Dispose();
                _fullScreen = null;
            }
            return result;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern static bool NativeChangeOrientation(DisplayOrientation Orientation);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern static uint NativeInitSpi(byte spiBus, int chipSelect, int dataCommand, int reset, int backLight, ushort x, ushort y, ushort width, ushort height, uint bufferSize);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern static uint NativeInitI2c(byte i2cBus, byte address, bool fastMode, ushort x, ushort y, ushort width, ushort height, uint bufferSize);

    }
}

