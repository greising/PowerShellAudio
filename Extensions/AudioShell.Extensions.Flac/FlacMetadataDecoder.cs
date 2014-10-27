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

using AudioShell.Extensions.Flac.Properties;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;

namespace AudioShell.Extensions.Flac
{
    [MetadataDecoderExport(".flac")]
    public class FlacMetadataDecoder : IMetadataDecoder
    {
        public MetadataDictionary ReadMetadata(Stream stream)
        {
            Contract.Ensures(Contract.Result<MetadataDictionary>() != null);

            using (var decoder = new NativeStreamMetadataDecoder(stream))
            {
                decoder.SetMetadataRespond(MetadataType.VorbisComment);

                DecoderInitStatus initStatus = decoder.Initialize();
                if (initStatus != DecoderInitStatus.OK)
                    if (initStatus == DecoderInitStatus.UnsupportedContainer)
                        throw new UnsupportedAudioException(Resources.MetadataDecoderUnsupportedContainerError);
                    else
                        throw new IOException(string.Format(CultureInfo.CurrentCulture, Resources.MetadataDecoderInitializationError, initStatus));

                if (!decoder.ProcessMetadata())
                    throw new IOException(string.Format(CultureInfo.CurrentCulture, Resources.MetadataDecoderDecodingError, decoder.GetState()));

                decoder.Finish();

                return decoder.Metadata == null ? new MetadataDictionary() : decoder.Metadata;
            }
        }
    }
}
