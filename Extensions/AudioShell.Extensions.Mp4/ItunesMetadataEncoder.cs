﻿/*
 * Copyright © 2014 Jeremy Herbison
 * 
 * This file is part of AudioShell.
 * 
 * AudioShell is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General
 * Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option)
 * any later version.
 * 
 * AudioShell is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public License for more
 * details.
 * 
 * You should have received a copy of the GNU Lesser General Public License along with AudioShell.  If not, see
 * <http://www.gnu.org/licenses/>.
 */

using AudioShell.Extensions.Mp4.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AudioShell.Extensions.Mp4
{
    [MetadataEncoderExport(".m4a")]
    public class ItunesMetadataEncoder : IMetadataEncoder
    {
        public SettingsDictionary DefaultSettings
        {
            get
            {
                Contract.Ensures(Contract.Result<SettingsDictionary>() != null);

                var result = new SettingsDictionary();

                result.Add("AddMetadata", bool.TrueString);
                result.Add("AddSoundCheck", bool.FalseString);

                return result;
            }
        }

        public IReadOnlyCollection<string> AvailableSettings
        {
            get
            {
                Contract.Ensures(Contract.Result<IReadOnlyCollection<string>>() != null);

                var result = new List<string>(3);

                result.Add("AddMetadata");
                result.Add("AddSoundCheck");
                result.Add("CreationTime");

                return result.AsReadOnly();
            }
        }

        public void WriteMetadata(Stream stream, MetadataDictionary metadata, SettingsDictionary settings)
        {
            var originalMp4 = new Mp4(stream);
            AtomInfo[] topAtoms = originalMp4.GetChildAtomInfo();

            // Create a temporary stream to hold the new atom structure:
            using (var tempStream = new MemoryStream())
            {
                var tempMp4 = new Mp4(tempStream);

                // Copy the ftyp and moov atoms to the temporary stream:
                originalMp4.CopyAtom(topAtoms.Single(atom => atom.FourCC == "ftyp"), tempStream);
                originalMp4.CopyAtom(topAtoms.Single(atom => atom.FourCC == "moov"), tempStream);

                // If there is metadata to write, append the atoms:
                if (string.IsNullOrEmpty(settings["AddMetadata"]) || string.Compare(settings["AddMetadata"], bool.TrueString, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    var adaptedMetadata = new MetadataToAtomAdapter(metadata, settings);
                    if (adaptedMetadata.Count > 0)
                    {
                        tempMp4.DescendToAtom("moov", "udta", "meta", "ilst");

                        tempStream.Position = tempMp4.CurrentAtom.End;

                        foreach (var item in adaptedMetadata)
                        {
                            byte[] atomData = item.GetBytes();
                            tempStream.Write(atomData, 0, atomData.Length);
                        }

                        tempMp4.UpdateAtomSizes((uint)tempStream.Length - tempMp4.CurrentAtom.End);
                    }
                }
                else if (string.Compare(settings["AddMetadata"], bool.FalseString, StringComparison.OrdinalIgnoreCase) != 0)
                    throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture, Resources.MetadataEncoderBadAddMetadata, settings["AddMetadata"]));

                // Update the creation times if they're being set explicitly:
                DateTime creationTime;
                if (!string.IsNullOrEmpty(settings["CreationTime"]))
                    if (DateTime.TryParse(settings["CreationTime"], out creationTime))
                        UpdateCreationTimes(creationTime, tempMp4);
                    else
                        throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture, Resources.MetadataEncoderBadCreationTime, settings["CreationTime"]));

                // Update the stco atom to reflect the new location of mdat:
                int mdatOffset = (int)(tempStream.Length - topAtoms.Single(atom => atom.FourCC == "mdat").Start);
                tempMp4.UpdateStco(mdatOffset);

                // Copy the mdat atom to the temporary stream, after the moov atom:
                tempStream.Seek(0, SeekOrigin.End);
                originalMp4.CopyAtom(topAtoms.Single(atom => atom.FourCC == "mdat"), tempStream);

                // Overwrite the original stream with the new, optimized one:
                stream.SetLength(tempStream.Length);
                stream.Position = 0;
                tempStream.Position = 0;
                tempStream.CopyTo(stream);
            }
        }

        static void UpdateCreationTimes(DateTime creationTime, Mp4 mp4)
        {
            Contract.Requires(mp4 != null);

            mp4.UpdateMvhd(creationTime, creationTime);
            mp4.UpdateTkhd(creationTime, creationTime);
            mp4.UpdateMdhd(creationTime, creationTime);
        }
    }
}
