﻿/*
 * Copyright © 2014, 2015 Jeremy Herbison
 * 
 * This file is part of PowerShell Audio.
 * 
 * PowerShell Audio is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser
 * General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your
 * option) any later version.
 * 
 * PowerShell Audio is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
 * implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public License
 * for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License along with PowerShell Audio.  If not, see
 * <http://www.gnu.org/licenses/>.
 */

using System.Collections;
using System.Management.Automation;

namespace PowerShellAudio.Commands
{
    [Cmdlet(VerbsCommon.Set, "AudioFileMetadata"), OutputType(typeof(TaggedAudioFile))]
    public class SetAudioFileMetadataCommand : Cmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public Hashtable Metadata { get; set; }

        [Parameter(Mandatory = true, Position = 1, ValueFromPipeline = true)]
        public AudioFile AudioFile { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        protected override void ProcessRecord()
        {
            var taggedAudioFile = new TaggedAudioFile(AudioFile);
            new HashTableToMetadataDictionaryAdapter(Metadata).CopyTo(taggedAudioFile.Metadata);
            if (PassThru)
                WriteObject(taggedAudioFile);
        }
    }
}
