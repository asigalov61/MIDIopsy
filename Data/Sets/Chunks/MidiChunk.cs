﻿/* MidiChunk.cs - Implementation of MidiChunk class, which represents a MIDI file chunk.
 *
 * Copyright (c) 2018-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* ApplicationException, Environment */
using System;

/* List */
using System.Collections.Generic;


namespace JeffBourdier
{
    /// <summary>Represents a MIDI file chunk (essentially, a collection of MidiItem objects).</summary>
    public class MidiChunk : MidiSet
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiChunk class.</summary>
        /// <param name="owner">The MIDI file to which this chunk belongs.</param>
        /// <param name="type">Four-character ASCII chunk type (e.g., "MThd" or "MTrk").</param>
        /// <param name="length">Number of bytes in the chunk (not including the eight bytes of type and length).</param>
        /// <param name="bytes">Array of bytes containing the chunk data.</param>
        /// <param name="index">
        /// Index in the byte array at which the chunk data begins (not including the eight bytes of type and length).
        /// </param>
        /// <remarks>A strongly typed class must be used for a chunk of known type (e.g., "MThd" or "MTrk").</remarks>
        public MidiChunk(MidiFile owner, string type, int length, byte[] bytes, int index)
            : base(new List<MidiItem>())
        {
            if (type == "MThd" || type == "MTrk") throw new ApplicationException(Properties.Resources.StronglyTypedClass);
            this._file = owner;
            MidiChunkInfo info = new MidiChunkInfo(type, length);
            this.AddItem(info);
            MidiItem item = new MidiItem(bytes, length, index);
            this.AddItem(item);
        }

        #endregion

        #region Protected Constructors

        /// <summary>Initializes a new instance of the MidiChunk class.</summary>
        /// <param name="owner">The MIDI file to which this chunk belongs.</param>
        /// <param name="type">Four-character ASCII chunk type (e.g., "MThd" or "MTrk").</param>
        /// <param name="length">Number of bytes in the chunk (not including the eight bytes of type and length).</param>
        protected MidiChunk(MidiFile owner, string type, int length)
            : base(new List<MidiItem>())
        {
            this._file = owner;
            MidiChunkInfo info = new MidiChunkInfo(type, length);
            this.AddItem(info);
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private MidiFile _file;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Gets the MIDI file to which this chunk belongs (set by constructor).</summary>
        public MidiFile File { get { return this._file; } }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Public Methods

        /// <summary>Gets the MidiItem object at the specified index.</summary>
        /// <param name="index">The zero-based index of the MidiItem to get.</param>
        /// <returns>The MidiItem object at the specified index.</returns>
        public MidiItem GetItem(int index) { return this.GetData(index) as MidiItem; }

        /// <summary>Gets the key signature at a given (cumulative) time.</summary>
        /// <param name="time">The cumulative time (in ticks) at which to get the key signature.</param>
        /// <returns>The key signature at the specified time.</returns>
        public override MidiKeySignature GetKeySignature(int time)
        {
            MidiKeySignature keySignature = base.GetKeySignature(time);
            if (keySignature == MidiKeySignature.NA) keySignature = this.File.GetKeySignature(time);
            return keySignature;
        }

        #endregion

        #region Protected Methods

        protected void AddItem(MidiItem item)
        {
            this.AddData(item);
            this.AddIndex(-1);
            this.AddHex(item.Hex + Environment.NewLine);
            this.AddComments(item.Comment + Environment.NewLine);
        }

        #endregion
    }
}
