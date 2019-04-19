﻿/* MidiChunk.cs - Implementation of MidiChunk class, which represents a MIDI file chunk.
 *
 * Copyright (c) 2018-9 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* ApplicationException */
using System;

/* List */
using System.Collections.Generic;


namespace JeffBourdier
{
    /// <summary>Represents a MIDI file chunk (essentially, a collection of MidiData objects).</summary>
    public class MidiChunk
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiChunk class.</summary>
        /// <param name="type">Four-character ASCII chunk type (e.g., "MThd" or "MTrk").</param>
        /// <param name="length">Number of bytes in the chunk (not including the eight bytes of type and length).</param>
        /// <param name="bytes">Array of bytes containing the chunk data.</param>
        /// <param name="index">
        /// Index in the byte array at which the chunk data begins (not including the eight bytes of type and length).
        /// </param>
        /// <remarks>A strongly typed class must be used for a chunk of known type (e.g., "MThd" or "MTrk").</remarks>
        public MidiChunk(string type, int length, byte[] bytes, int index)
        {
            if (type == "MThd" || type == "MTrk") throw new ApplicationException(Properties.Resources.StronglyTypedClass);
            MidiChunkInfo info = new MidiChunkInfo(type, length);
            this.AddData(info);
            MidiData data = new MidiData(bytes, length, index);
            this.AddData(data);
        }

        #endregion

        #region Protected Constructors

        /// <summary>Initializes a new instance of the MidiChunk class.</summary>
        /// <param name="type">Four-character ASCII chunk type (e.g., "MThd" or "MTrk").</param>
        /// <param name="length">Number of bytes in the chunk (not including the eight bytes of type and length).</param>
        protected MidiChunk(string type, int length)
        {
            MidiChunkInfo info = new MidiChunkInfo(type, length);
            this.AddData(info);
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private List<MidiData> Data = new List<MidiData>();
        private string _Hex;
        private string _Comments;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Gets the number of MidiData objects contained in this chunk.</summary>
        public int DataCount { get { return this.Data.Count; } }

        /// <summary>Representation of all MidiData objects contained in this chunk, in hexadecimal format.</summary>
        public string Hex { get { return this._Hex; } }

        /// <summary>Human-readable text describing each MidiData object contained in this chunk.</summary>
        public string Comments { get { return this._Comments; } }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Public Methods

        /// <summary>Gets the MidiData object at the specified index.</summary>
        /// <param name="index">The zero-based index of the MidiData object to get.</param>
        /// <returns>The MidiData object at the specified index.</returns>
        public MidiData GetData(int index) { return this.Data[index]; }

        #endregion

        #region Protected Methods

        protected void AddData(MidiData data)
        {
            this.Data.Add(data);
            this._Hex += data.Hex + Environment.NewLine;
            this._Comments += data.Comment + Environment.NewLine;
        }

        #endregion
    }
}
