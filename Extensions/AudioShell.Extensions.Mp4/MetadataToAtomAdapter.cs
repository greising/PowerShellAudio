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

namespace AudioShell.Extensions.Mp4
{
    class MetadataToAtomAdapter : List<IWritableAtom>
    {
        static readonly Dictionary<string, string> _map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            { "Album", "©alb"   },
            { "Artist", "©ART"  },
            { "Comment", "©cmt" },
            { "Genre", "©gen"   },
            { "Title", "©nam"   },
            { "Year", "©day"    }
        };

        internal MetadataToAtomAdapter(MetadataDictionary metadata, SettingsDictionary settings)
        {
            Contract.Requires(metadata != null);

            var trackNumberAtom = new TrackNumberAtom();
            var trackSoundCheckAtom = new SoundCheckAtom();
            var albumSoundCheckAtom = new SoundCheckAtom();

            foreach (var item in metadata)
            {
                switch (item.Key)
                {
                    case "TrackNumber":
                        trackNumberAtom.TrackNumber = byte.Parse(item.Value, CultureInfo.InvariantCulture);
                        break;

                    case "TrackCount":
                        trackNumberAtom.TrackCount = byte.Parse(item.Value, CultureInfo.InvariantCulture);
                        break;

                    case "TrackGain":
                        trackSoundCheckAtom.Gain = item.Value;
                        break;

                    case "TrackPeak":
                        trackSoundCheckAtom.Peak = item.Value;
                        break;

                    case "AlbumGain":
                        albumSoundCheckAtom.Gain = item.Value;
                        break;

                    case "AlbumPeak":
                        albumSoundCheckAtom.Peak = item.Value;
                        break;

                    default:
                        string fourCC;
                        if (_map.TryGetValue(item.Key, out fourCC))
                            Add(new TextAtom(fourCC, item.Value));
                        break;
                }
            }

            if (trackNumberAtom.IsValid)
                Add(trackNumberAtom);

            if (!string.IsNullOrEmpty(settings["AddSoundCheck"]) && string.Compare(settings["AddSoundCheck"], bool.FalseString, StringComparison.OrdinalIgnoreCase) != 0)
            {
                if (string.Compare(settings["AddSoundCheck"], "Album", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (string.IsNullOrEmpty(albumSoundCheckAtom.Gain))
                        throw new InvalidSettingException(Resources.MetadataToAtomAdapterMissingAlbumGain);
                    if (string.IsNullOrEmpty(albumSoundCheckAtom.Peak))
                        throw new InvalidSettingException(Resources.MetadataToAtomAdapterMissingAlbumPeak);

                    Add(albumSoundCheckAtom);
                }
                else if (string.Compare(settings["AddSoundCheck"], "Track", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (string.IsNullOrEmpty(trackSoundCheckAtom.Gain))
                        throw new InvalidSettingException(Resources.MetadataToAtomAdapterMissingTrackGain);
                    if (string.IsNullOrEmpty(trackSoundCheckAtom.Peak))
                        throw new InvalidSettingException(Resources.MetadataToAtomAdapterMissingTrackPeak);

                    Add(trackSoundCheckAtom);
                }
                else
                    throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture, Resources.MetadataToAtomAdapterBadAddSoundCheck, settings["AddSoundCheck"]));
            }
        }
    }
}
