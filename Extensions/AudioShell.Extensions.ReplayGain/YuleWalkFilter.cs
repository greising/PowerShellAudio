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

using System.Diagnostics.CodeAnalysis;

namespace AudioShell.Extensions.ReplayGain
{
    class YuleWalkFilter : EqualLoudnessFilter
    {
        [SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member", Justification = "Does not waste space")]
        static readonly float[,] _a = {{ 0.0385759944f, -0.0216036718f, -0.00123395317f, -0.0000929167796f,  -0.0165526034f,   0.0216152684f,  -0.0207404522f,  0.00594298065f,     0.00306428023f, 0.000120253220f,  0.00288463684f },
                                       { 0.0541865641f, -0.0291100781f, -0.00848709380f,   -0.00851165645f, -0.00834990905f,   0.0224529325f,  -0.0259633851f,   0.0162486496f,    -0.00240879052f,  0.00674613682f, -0.00187763777f },
                                       {  0.102967172f, -0.0487797558f,  -0.0287800908f,    -0.0351950919f,   0.0288871717f, -0.00609872685f,  0.00209851217f,  0.00911704669f,      0.0115440472f, -0.00630293689f,  0.00107527155f },
                                       {  0.115722970f, -0.0412091605f,  -0.0497773177f,    -0.0104730868f,  0.00750863219f, 0.000555076944f,  0.00140344193f,   0.0128609525f,     0.00998223034f, -0.00725013811f,  0.00326503347f },
                                       {  0.154572997f, -0.0933104906f,  -0.0624788015f,     0.0216354189f,  -0.0558839333f,   0.0478147667f,  0.00222312598f,   0.0317409254f,     -0.0139058942f,  0.00651420668f, -0.00881362734f },
                                       {  0.238823923f,  -0.220077915f,  -0.0601458195f,     0.0500445806f,  -0.0329311125f,   0.0234867819f,   0.0429054980f, -0.00938141862f,    0.000150951463f, -0.00712601541f, -0.00626520210f },
                                       {  0.302969073f,  -0.226139887f,  -0.0858732373f,     0.0328293017f, -0.00915702933f,  -0.0236414120f, -0.00584456040f,   0.0627610132f, -0.00000828086748f,  0.00205861886f,  -0.0295013498f },
                                       {  0.336423049f,  -0.255722414f,   -0.118285702f,      0.119211487f,  -0.0783448961f, -0.00469977914f, -0.00589500224f,   0.0572422814f,     0.00832043981f,  -0.0163538138f,  -0.0176017657f },
                                       {  0.384126573f,   0.445337296f,    0.204266381f,     -0.280316760f,    0.314842026f,   -0.260783112f,    0.129252012f,  -0.0114116470f,      0.0303652212f,  -0.0377633931f,  0.00692036604f },
                                       {  0.449152566f,  -0.143517575f,   -0.227843944f,    -0.0141914010f,   0.0407826280f,   -0.123981634f,   0.0409756514f,    0.104785036f,     -0.0186388781f,  -0.0319342844f,  0.00541907749f },
                                       {  0.566194708f,  -0.754644569f,    0.162421377f,      0.167442435f,   -0.189016042f,    0.309317828f,   -0.275629620f,  0.00647310677f,      0.0864750378f,  -0.0378898455f, -0.00588215443f },
                                       {  0.581004950f,  -0.531749091f,   -0.142897990f,      0.175207048f,   0.0237794522f,    0.155584491f,   -0.253447901f,   0.0162846241f,      0.0692046776f,  -0.0372161140f, -0.00749618797f },
                                       {  0.536487893f,  -0.421630344f, -0.00275953612f,     0.0426784222f,   -0.102148642f,    0.145907723f,  -0.0245986486f,   -0.112023152f,     -0.0406003413f,   0.0478866555f,  -0.0221793680f }};

        [SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member", Justification = "Does not waste space")]
        static readonly float[,] _b = {{  -3.84664617f,    7.81501653f,    -11.3417036f,       13.0550422f,    -12.2875990f,     9.48293806f,    -5.87257862f,     2.75465862f,      -0.869843766f,    0.139193145f },
                                       {  -3.47845949f,    6.36317779f,    -8.54751527f,       9.47693608f,    -8.81498681f,     6.85401541f,    -4.39470996f,     2.19611685f,      -0.751043025f,    0.131493180f },
                                       {  -2.64848055f,    3.58406058f,    -3.83794914f,       3.90142346f,    -3.50179819f,     2.67085284f,    -1.82581142f,     1.09530368f,      -0.476890178f,    0.111714315f },
                                       {  -2.43606803f,    3.01907407f,    -2.90372016f,       2.67947188f,    -2.17606479f,     1.44912957f,   -0.877857655f,    0.535922027f,      -0.264693448f,   0.0749587806f },
                                       {  -2.37898835f,    2.84868151f,    -2.64577170f,       2.23697657f,    -1.67148153f,     1.00595955f,   -0.459534581f,    0.163781649f,     -0.0503207772f,   0.0234789741f },
                                       {  -2.06894081f,    1.76944700f,   -0.814047326f,      0.254182869f,   -0.303407917f,    0.356168841f,   -0.149673106f,  -0.0702415418f,       0.110784043f,  -0.0355183800f },
                                       {  -1.61273165f,    1.07977492f,   -0.256562578f,     -0.162767191f,   -0.226388938f,    0.391208008f,   -0.221381390f,   0.0450023539f,      0.0200585181f,  0.00302439096f },
                                       {  -1.49858979f,   0.873502714f,    0.122050223f,     -0.807749447f,    0.478547946f,   -0.124534581f,  -0.0406751020f,   0.0833375528f,     -0.0423734803f,   0.0297720732f },
                                       {  -1.74403916f,    1.96686096f,    -2.10081453f,       1.90753918f,    -1.83814264f,     1.36971352f,   -0.778836091f,    0.392664225f,      -0.125293836f,   0.0542476070f },
                                       { -0.628206192f,   0.296617837f,   -0.372563729f,    0.00213767857f,   -0.420298202f,    0.221996506f,  0.00613424351f,   0.0674762074f,      0.0578482038f,   0.0322275407f },
                                       {  -1.04800335f,   0.291563120f,   -0.268060010f,    0.00819999646f,    0.450547345f,   -0.330324033f,   0.0673936833f,  -0.0478425423f,      0.0163990784f,   0.0180736432f },
                                       { -0.510353271f,  -0.318635633f,   -0.202564135f,      0.147281541f,    0.389526400f,   -0.233132719f,  -0.0524601902f,  -0.0250596172f,      0.0244235732f,   0.0181880111f },
                                       { -0.250498720f,  -0.431939423f,  -0.0342468102f,    -0.0467832878f,    0.264083002f,    0.151131305f,   -0.175564934f,   -0.188230093f,      0.0547772043f,   0.0470440969f }};

        internal YuleWalkFilter(int sampleRate)
            : base(sampleRate, _a, _b)
        { }
    }
}
