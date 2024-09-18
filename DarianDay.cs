﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DarianDay
{
	class DarianDay
	{
		static readonly double MARS_TO_EARTH_DAYS = 1.027491251;
		static readonly double EPOCH_OFFSET = 587744.77817;
		static readonly double ROUND_UP_SECOND = 1 / 86400;
		static readonly int[] eDaysTilMonth = [ -1, -1, 30, 58, 89, 119, 150, 180, 211, 242, 272, 303, 333 ];
		static readonly int[] eDaysInMonth = [ 0, 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 ];
		static readonly string[] mSolNomens = [
			"Sol Solis", "Sol Lunae", "Sol Martis", "Sol Mercurii", "Sol Jovis", "Sol Veneris", "Sol Saturni", // Standard Darian nomenclature
			"Axatisol", "Benasol", "Ciposol", "Domesol", "Erjasol", "Fulisol", "Gavisol", // Darian Defrost nomenclature
			"Sunsol", "Phobosol", "Deimosol", "Terrasol", "Venusol", "Mercurisol", "Jovisol" // Standard Utopian nomenclature
		];

		static int mYear;
		static int mMonth;
		static string mMonthName;
		static int mDay;
		static int mDayOfWeek;
		static int mHour;
		static int mMin;
		static int mSec;
		static int mMil;

		static string mSolName;
		static string eDayName;

		static int marsYear;
		static int earthYear;

		static int eYear;
		static int eMonth;
		static string eMonthName;
		static int eDay;
		static int eDayOfWeek;
		static int eHour;
		static int eMin;
		static int eSec;
		static int eMil;

		static string ThisDay;
		static string ThisSol;

		static int LeapDay;

		static string InSolNomen;

		static bool DateSuccess = true;
		static bool earthDate = true;

		static int cLineCount = 0;

		static int AToI(string s)
		{
			int iOut = 0;

			char[] cS = s.ToCharArray();

			for (int i = 0; i < s.Length; i++)
				iOut += (cS[i] - 48) * Pow(10, s.Length - (i + 1));

			return iOut;
		}

		static int Pow(int x, int y)
		{
			int iOut = x;

			if (y == 0)
				return 1;

			if (y < 0)
				iOut = 1;

			for (int i = 0; i < y; i++)
			{
				if (y < 0)
					iOut /= x;

				if (y > 0)
					iOut *= x;
			}

			return iOut;
		}

		static void FormatWrite(string line)
		{
			var lines = line.Split('\n');
			var width = Console.BufferWidth - 1;

			for (int i = 0; i < lines.GetLength(0); i++)
			{
				var top = Console.CursorTop;
				Console.SetCursorPosition(0, top);
				var tmpLine = lines[i];
				while (tmpLine.Length > width)
				{
					for (int j = width - 1; j > -1; j--)
					{
						if (tmpLine[j] == ' ')
						{
							Console.Write(tmpLine[..j]);
							Console.Write(new string(' ', Console.BufferWidth - Console.CursorLeft));
							Console.SetCursorPosition(0, top + 1);
							top = Console.CursorTop;
							tmpLine = tmpLine[(j + 1)..];
							j = -1;
						}
					}
				}

				Console.Write(tmpLine);
				Console.Write(new string(' ', Console.BufferWidth - Console.CursorLeft));
				Console.SetCursorPosition(0, top + 1);
			}
		}

		static double GetEarthDaysFromForm()
		{
			var year = eYear;
			var month = eMonth;
			var day = eDay;

			double daysSince = (double)(day + eDaysTilMonth[month])
			+ 365 * year
			+ Math.Floor(year / 4.0)
			- Math.Floor(year / 100.0)
			+ Math.Floor(year / 400.0);

			if ((month < 3)
			&& IsEarthLeapYear(year))
				daysSince -= 1;

			double hour = (double)(eHour / 24.0);
			double min = (double)(eMin / 1440.0);
			double sec = (double)(eSec / 86400.0);
			double mil = (double)(eMil / 86400000.0);

			daysSince += hour;
			daysSince += min;
			daysSince += sec;
			daysSince += mil;

			return daysSince;
		}

		static double GetMarsSolsFromForm()
		{
			var year = mYear;
			var month = mMonth;
			var day = mDay;

			double solsSince = (double)(day + ((month - 1) * 28) - Math.Floor((double)((month - 1) / 6))
			+ 668 * year
			+ Math.Floor(year / 2.0)
			+ Math.Floor((year - 1) / 10.0)
			- Math.Floor((year - 1) / 100.0)
			+ Math.Floor((year - 1) / 1000.0));

			double hour = (double)(mHour / 24.0);
			double min = (double)(mMin / 1440.0);
			double sec = (double)(mSec / 86400.0);
			double mil = (double)(mMil / 86400000.0);

			solsSince += hour;
			solsSince += min;
			solsSince += sec;
			solsSince += mil;

			return solsSince;
		}

		static void SetMarsDateFromSols(double solsSince)
		{
			// get the fractional part, to do the time later
			var partialSol = solsSince - Math.Floor(solsSince);

			// Convert sols to Martian date:
			var s = solsSince;

			var sD = Math.Floor(s / 334296);
			int doD = (int)Math.Floor(s - (sD * 334296));

			double sC = 0.0;
			var doC = doD;
			if (doD != 0) sC = (int)Math.Floor((double)((doD - 1) / 66859));
			if (sC != 0) doC -= (int)(sC * 66859 + 1);
			var doX = doC;

			int sX;
			if (sC != 0)    // century that does not begin with leap day
			{
				sX = (int)Math.Floor((double)((doC + 1) / 6686));
				if (sX != 0) doX -= sX * 6686 - 1;
			}
			else
			{
				sX = (int)Math.Floor((double)(doC / 6686));
				if (sX != 0) doX -= sX * 6686;
			}

			var sII = 0;
			var doII = doX;
			if (sC != 0 && sX == 0) // decade that does not begin with leap day
			{
				sII = (int)Math.Floor((double)(doX / 1337));
				if (sII != 0) doII -= sII * 1337;
			}
			else    // 1338, 1337, 1337, 1337 ...
			{
				if (doX != 0) sII = (int)Math.Floor((double)((doX - 1) / 1337));
				if (sII != 0) doII -= sII * 1337 + 1;
			}

			int doI = doII;

			int sI;
			if (sII == 0 && (sX != 0 || (sX == 0 && sC == 0)))
			{
				sI = (int)Math.Floor((double)(doII / 669));
				if (sI != 0) doI -= 669;
			}
			else    // 668, 669
			{
				sI = (int)Math.Floor((double)((doII + 1) / 669));
				if (sI != 0) doI -= 668;
			}

			marsYear = (int)(500 * sD + 100 * sC + 10 * sX + 2 * sII + sI);
			var leap = marsYear % 2 == 1 || marsYear % 10 == 0;

			// get the date from the day of the year:
			var tmpSeason = GetMartianSeasonFromSol(doI);   // 0-3
			var tmpSolOfSeason = doI - 167 * tmpSeason; // 0-167
			var tmpMonthOfSeason = Math.Floor((double)(tmpSolOfSeason / 28));   // 0-5
			var marsMonth = tmpMonthOfSeason + 6 * tmpSeason + 1;   // 1-24
			var marsDay = doI - ((marsMonth - 1) * 28 - tmpSeason) + 1; // 1-28

			// There is a bug which causes a sol to be subtracted every 1,000 Martian years starting from the year 501.
			// The cause is as yet unknown, but accounted for here.
			var correctionTmp = 0.5;
			var yearTmp = marsYear;
			while (yearTmp - 500 > 0)
			{
				correctionTmp += 0.5;
				yearTmp -= 500;
			}

			var shortMonth = marsMonth % 6 == 0 && (marsMonth != 24 || leap == false);

			marsDay += Math.Floor(correctionTmp);
			doI += (int)Math.Floor(correctionTmp);
			while (marsDay > 28 || (marsDay > 27 && shortMonth))
			{
				marsDay -= shortMonth ? 27 : 28;
				marsMonth += 1;
				if (marsMonth > 24)
				{
					marsYear += 1;
					marsMonth -= 24;
					doI -= leap ? 669 : 668;
				}
				leap = marsYear % 2 == 1 || marsYear % 10 == 0;
				shortMonth = marsMonth % 6 == 0 && (marsMonth != 24 || leap == false);
			}

			var SolNomen = AToI(InSolNomen);

			mDayOfWeek = (int)((marsDay - 1) % 7 + 1);
			string nSolName = mSolNomens[((SolNomen - 1) * 7) + mDayOfWeek - 1];

			// Put the result up:
			mYear = marsYear;
			mMonth = (int)marsMonth;
			mDay = (int)marsDay;
			mSolName = nSolName;

			var tmpHour = partialSol * 24;
			var tmpMin = (tmpHour - Math.Floor(tmpHour)) * 60;
			var tmpSec = (tmpMin - Math.Floor(tmpMin)) * 60;
			var tmpMil = (tmpSec - Math.Floor(tmpSec)) * 1000;
			mHour = (int)Math.Floor(tmpHour);
			mMin = (int)tmpMin;
			mSec = (int)tmpSec;
			mMil = (int)tmpMil;

			ThisSol = doI switch
			{
				0 => "Vernal Equinox (nominal date).\nOn this sol in 160: The first motion picture about Mars, \"A Trip to Mars,\" was released.",
				1 => "On this sol in 222: Approximate time setting for \"October 2026: The Million-Year Picnic\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				3 => "On this sol in 136: Robert G. Aitken was born, developed the first complete Martian calendar.",
				4 => "On this sol in 188: Mars 1962A (Sputnik 29) was launched, but failed to leave Earth orbit.",
				5 => "On this sol in 170: Philip K. Dick was born, author of \"Martian Time-Slip.\"",
				6 => "On this sol in 162: Edgar Rice Burroughs` \"The Warlord of Mars\" began serialization in \"All-Story\" (date approximate).",
				8 => "On this sol in 194: Mars 4 failed to enter Mars orbit, passed Mars. The motion picture \"Conquest of Space\" was released.",
				10 => "On this sol in 194: Mars 5 entered Mars orbit.",
				12 => "On this sol in 188: Mars 1 was launched. Robert A. Heinlein`s \"Podkayne of Mars\" began serialization in \"If\" (date approximate).",
				14 => "On this sol in 184: The motion picture \"Devil Girl From Mars\" was released.",
				15 => "On this sol in 188: Mars 1962B (Sputnik 31) was launched, but failed to leave Earth orbit.\nOn this sol in 215: The Sixteenth International Mars Society Convention opened in Boulder, Colorado.",
				16 => "On this sol in 208: A meteorite, designated Dhofar 378, was discovered near Dhofar, Oman. The meteorite was later identified as having originated on Mars.\nOn this sol in 214: The Eleventh European Mars Convention opened in Neuchatel, Switzerland.",
				18 => "On this sol in 019: Two patches on the lower part of the disk of Mars were described by a Neapolitan Jesuit named Father Bartoli.",
				23 => "On this sol in 206: US President Bill Clinton abandoned George Bush`s goal of a manned landing on Mars. His National Science and Technology Council`s statement of National Space Policy only mentioned \"a sustained program to support a robotic presence on the surface of Mars by year 2000 for the purposes of scientific research, exploration and technology development.\"",
				24 => "On this sol in 161: Edgar Rice Burroughs` \"Under the Moons of Mars\" began serialization in \"All-Story,\" later was published as a novel under the title \"A Princess of Mars\" (date approximate).",
				25 => "On this sol in 214: The motion picture \"Interplanetary\" was released.\nOn this sol in 171: The motion picture \"Just Imagine\" was released.",
				26 => "On this sol in 189: Leigh Brackett`s \"Purple Priestess of the Mad Moon\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).",
				28 => "On this sol in 154: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 29th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this sol in 207: \"The Founding Convention of the Mars Society\" opened in Boulder, Colorado.",
				30 => "On this sol in 207: \"The Founding Declaration of the Mars Society\" was adopted unanimously in Boulder, Colorado.",
				35 => "On this sol in 194: Mars 7 passed Mars. Lander missed Mars.",
				37 => "On this sol in 202: Contact with Phobos 2 was lost after 58 sols in Mars orbit.",
				38 => "On this sol in 194: Mars 6 passed Mars. Contact with the lander was lost before landing.",
				41 => "On this sol in 164: Edgar Rice Burroughs` \"A Princess of Mars\" was published by McClurg.",
				42 => "On this sol in 153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the seventh of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				46 => "On this sol in 145: Giovanni Schiaparelli recorded the first instance of gemination. \"Great was my astonishment on January 19, when, on examining the Jamuna... I saw instead of its usual appearance two straight and equal parallel lines running between the Niliacus Lacus and Aurorae Sinus. At first I believed this to be the deception of a tired eye, or perhaps the effect of some kind of strabismus, but I soon convinced myself that the phenomenon was real.\"",
				47 => "On this sol in 211: Mars Reconnaissance Orbiter entered Mars orbit.",
				50 => "On this sol in 191: NASA terminated the contract to procure additional Saturn Vs, ending production with Saturn 515, and abandoning the heavy-lift launch capability required to launch piloted Mars missions.\nOn this sol in 215: The motion picture \"The Last Days on Mars\" was released.",
				52 => "On this sol in 196: A meteorite, designated ALHA 77005, was discovered in the Allan Hills, Antarctica. In the 200s, the meteorite was identified as having originated on Mars.",
				54 => "On this sol in 161: Leigh Brackett was born, author of the Eric John Stark series of Martian stories.",
				55 => "On this sol in 189: \"The Outer Limits\" episode \"The Invisible Enemy\" aired.",
				59 => "On this sol in 171: The motion picture \"Mars\" was released.\nOn this sol in 189: Mariner 3 failed during launch.\nOn this sol in 215: The Thirteeth Australian Mars Exploration Conference opened in Sydney, New South Wales.",
				60 => "On this sol in 208: A meteorite, designated Sayh al Uhaymir 051, was discovered near Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.",
				63 => "On this sol in 213: The motion picture \"Princess of Mars\" was released.",
				65 => "On this sol in 153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the eighth of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				66 => "On this sol in 189: The motion picture \"Pajama Party\" (also titled \"The Maid and the Martian\") was released.\nOn this sol in 205: A meteorite, designated QUE 94201, was discovered in the Queen Alexandra Range, Antarctica. The meteorite was later identified as having originated on Mars.",
				70 => "On this sol in 206: Mars Global Surveyor was launched.\nOn this sol in 208: \"The Third International Mars Society Convention\" opened in Toronto, Ontario.",
				71 => "On this sol in 165: Edgar Rice Burroughs` \"The Warlord of Mars\" was published by McClurg.",
				72 => "On this sol in 214: Mars Science Laboratory \"Curiosity\" was launched.",
				74 => "On this sol in 171: Wernher von Braun was born, developed the Saturn V launch vehicle, author of \"The Mars Project.\"",
				78 => "On this sol in 206: The motion picture \"Space Jam\" starring Marvin the Martian was released.",
				79 => "On this sol in 206: Mars 96 failed during launch.",
				80 => "On this sol in 208: The Second International Conference on Mars Polar Science and Exploration opened in Reykjavik, Iceland.",
				81 => "On this sol in 189: Mariner 4 was launched.\nOn this sol in 206: The third annual \"Lunar and Mars Exploration\" conference was held in San Diego, California.",
				82 => "On this sol in 205: Jerry Oltion`s \"The Great Martian Pyramid Hoax\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).",
				83 => "On this sol in 127: Kurd Lasswitz was born, author of \"Two Planets.\"\nOn this sol in 153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the ninth of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this sol in 189: Zond 2 was launched.\nOn this sol in 209: The Second Australian Mars Exploration Conference opened in Sydney, New South Wales.",
				84 => "On this sol in 215: The Thirteenth European Mars Convention opened in Paris, France.",
				88 => "On this sol in 173: Edgar Rice Burroughs` \"The Swords of Mars\" began serialization in \"Blue Book\" (date approximate).",
				90 => "On this sol in 183: The motion picture \"The War of the Worlds\" was released.",
				92 => "On this sol in 183: The motion picture \"The War of the Worlds\" was released.",
				93 => "On this sol in 045: Christiaan Huygens drew his last sketch of Mars.",
				94 => "On this sol in 182: \"Tales of Tomorrow\" episode \"The Crystal Egg\" aired.\nOn this sol in 207: The First International Conference on Mars Polar Science and Exploration opened in Houston, Texas.",
				95 => "On this sol in 173: Carl Sagan was born, first president of the Planetary Society.\nOn this sol in 215: Mars Orbiter Mission (MOM), also called Mangalyaan, was launched.",
				96 => "On this sol in 206: Mars Pathfinder was launched.",
				98 => "On this sol in 198: The Third International Colloquium on Mars opened in Pasadena, California.\nOn this sol in 209: Alex Irvine`s \"Pictures from an Expedition\" was published in \"In The Magazine of Fantasy and Science Fiction.\"",
				100 => "On this sol in 153: H. G. Wells` \"The War of the Worlds\" began serialization in England in \"Pearson`s\" (date approximate).",
				103 => "On this sol in 209: Approximate time setting for \"August 2002: Night Meeting\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				104 => "On this sol in 201: \"The Case For Mars III\" conference opened in Boulder, Colorado.\nOn this sol in 2013: US President Barack Obama canceled the Constellation program of human expeditions to the Moon and Mars.",
				106 => "On this sol in 206: The motion picture \"Mars Attacks!\" was released.",
				107 => "On this sol in 164: Arthur C. Clarke was born, author of \"The Sands of Mars.\"\nOn this sol in 208: \"The Fifth International Mars Society Convention\" opened in Boulder, Colorado.",
				108 => "On this sol in 215: Mars Atmosphere and Volatile EvolutioN Mission (MAVEN) was launched.",
				109 => "On this sol in 197: A meteorite, designated EETA 79001, was discovered in the Elephant Moraine, Antarctica. The meteorite was later identified as having originated on Mars.",
				114 => "On this sol in 154: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 30th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				115 => "On this sol in 184: The motion picture \"Devil Girl From Mars\" was released.\nOn this sol in 188: The motion picture \"The Day Mars Invaded Earth\" was released.\nOn this sol in 203: \"America at the Threshold: America`s Space Exploration Initiative\" (\"The Stafford Report\") called for manned missions to Mars beginning in 215.",
				117 => "On this sol in 152: \"The San Francisco Chronicle\" reported that an observer had read the name of the Almighty in Hebrew letters on the surface of Mars.",
				118 => "On this sol in 176: Edgar Rice Burroughs began writing \"John Carter and the Pits of Horz,\" first of a series for new book. The story was later was published under the title \"The City of Mummies.\"",
				122 => "On this sol in 208: Kim Stanley Robinson`s \"The Martians\" was published by Spectra.",
				123 => "On this sol in 182: The motion picture \"Flight to Mars\" was released.\nOn this sol in 197: The first episode of the miniseries \"The Martian Chronicles\" aired.",
				124 => "On this sol in 197: The second episode of the miniseries \"The Martian Chronicles\" aired.",
				125 => "On this sol in 197: The third and final episode of the miniseries \"The Martian Chronicles\" aired.",
				129 => "On this sol in 153: H. G. Wells` \"The War of the Worlds\" began serialization in the USA in \"Cosmopolitan\" (date approximate).",
				134 => "On this sol in 154: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 31st of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				135 => "On this sol in 206: Marvin the Martian appeared in \"The Springfield Files\" episode of the animated television series \"The Simpsons.\"",
				140 => "On this sol in 154: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 32nd of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this sol in 207: A meteorite, designated Yamato 980459, was discovered in the Yamato Mountains, Antarctica. The meteorite was later identified as having originated on Mars. \"The Outer Limits\" episode \"Phobos Rising\" aired.",
				142 => "On this sol in 154: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 33rd of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				143 => "On this sol in 213: Contact with Mars Exploration Rover A (Spirit) was lost.",
				144 => "On this sol in 207: Mars Climate Orbiter was launched.\nOn this sol in 210: The Fourth Australian Mars Exploration Conference opened in Adelaide, South Australia.",
				145 => "On this sol in 181: Japanese astronomer Sadao Saeki saw a huge explosion on Mars which produced a mushroom cloud 1450 km in diameter \"like the terrific explosion of a volcano.\" No other people observed this explosion.",
				148 => "On this sol in 188: Contact with Mars 1 was lost enroute to Mars.",
				149 => "On this sol in 202: US President George H. W. Bush launched the Space Exploration Initiative from the steps of the National Air & Space Museum on the 20th anniversary of the Apollo 11 moon landing, calling for \"a journey into tomorrow, a journey to another planet - a manned mission to Mars.\"",
				151 => "Aphelion (nominal date).\nOn this sol in 153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 10th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				156 => "On this sol in 204: NASA issued \"Mars Design Reference Mission 1.0\" (date approximate).",
				158 => "On this sol in 209: The Second European Mars Convention opened in Rotterdam, Netherlands.",
				159 => "On this sol in 208: The motion picture \"Red Planet\" was released.",
				162 => "On this sol in 209: Approximate time setting for \"October 2002: The Shore\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				163 => "On this sol in 210: \"The Seventh International Mars Society Convention\" opened in Chicago, Illinois.",
				164 => "On this sol in 165: Isaac Asimov was born, author of \"The Martian Way.\"\nOn this sol in 212: Phoenix landed in Green Valley, Vastitas Borealis.",
				166 => "On this sol in 214: The motion picture \"We Are One\" was released.",
				167 => "On this sol in 207: Mars Polar Lander was launched. Approximate time setting for \"January 1999: Rocket Summer\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				169 => "On this sol in 209: A meteorite, designated Sayh al Uhaymir 150, was discovered near Dar al Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.",
				170 => "On this sol in 202: \"Mars is essentially in the same orbit. Mars is somewhat the same distance from the sun, which is very important. We have seen pictures where there are canals, we believe, and water. If there is water, that means there is oxygen. If oxygen, that means we can breathe.\" -- US Vice President Dan Quayle",
				171 => "On this sol in 171: The Mercury Theater of the Air, starring Orson Welles, performed Howard Koch`s radio play \"Invasion from Mars.\" The broadcast terrorized the eastern USA.\nOn this sol in 213: The First International Conference on Mars Sedimentology and Stratigraphy opened in El Paso, Texas.",
				174 => "On this sol in 163: Edgar Rice Burroughs` \"Thuvia, Maid of Mars\" began serialization in \"All-Story Weekly.\"\nOn this sol in 214: The motion picture \"John Carter\" was released.",
				176 => "On this sol in 153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 11th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				177 => "On this sol in 195: Viking 1 entered Mars orbit.\nOn this sol in 208: A meteorite, designated Y000593, was discovered in the Yamato Mountains, Antarctica. The meteorite was later identified as having originated on Mars.",
				179 => "On this sol in 175: The motion picture \"Mars Attacks the World\" was released.",
				181 => "On this sol in 204: \"The Case For Mars V\" conference opened in Boulder, Colorado.\nOn this sol in 208: A meteorite, designated Y000749, was discovered in the Yamato Mountains, Antarctica. The meteorite was later identified as having originated on Mars.",
				182 => "On this sol in 176: Edgar Rice Burroughs began writing \"The Black Pirates of Barsoom,\" part 2 of a new Mars series.",
				185 => "On this sol in 153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 12th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				188 => "On this sol in 188: The television series \"My Favorite Martian\" ended with the 107th episode.",
				189 => "On this sol in 117: Asaph Hall was born, discoverer of Phobos and Deimos.\nOn this sol in 211: The Ninth International Mars Society Convention opened in Washington, DC.",
				190 => "On this sol in 195: Lin Carter`s \"The Martian El Dorado of Parker Wintley\" was published in \"The DAW Science Fiction Reader\" (date approximate).",
				193 => "Summer Solstice (nominal date).",
				195 => "On this sol in 157: \"The New York Times\" reported that Nikola Tesla might use an oscillator to \"wake up\" Mars.",
				196 => "On this sol in 197: Contact with Viking Lander 2 was lost after 1,280 sols in Utopia Planitia.",
				197 => "On this sol in 184: Fredric Brown`s \"Martians, Go Home\" was published (date approximate).\nOn this sol in 207: Approximate time setting for \"February 1999: Ylla\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				199 => "On this sol in 205: Kim Stanley Robinson`s \"Green Mars\" was published by Spectra.",
				202 => "On this sol in 212: The Eighth Australian Mars Exploration Conference opened in Adelaide, South Australia.",
				204 => "On this sol in 154: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the last of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				205 => "On this sol in 207: The motion picture \"My Favorite Martian\" was released.",
				208 => "On this sol in 176: Edgar Rice Burroughs began writing \"Escape on Mars,\" part 3 of a new Mars series. The story was later was published under the title \"The Yellow Men of Mars.\"\nOn this date in 195: Viking Lander 1 achieved first successful landing on Mars in Chryse Planitia. The spacecraft was later renamed the Thomas A. Mutch Memorial Station.\nOn this sol in 209: A meteorite, designated Sayh al Uhaymir 120, was discovered near Dar al Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.",
				209 => "On this sol in 171: Edgar Rice Burroughs` \"A Fighting Man of Mars\" was published by \"Metropolitan.\"",
				211 => "On this sol in 210: The Fourth European Mars Convention opened in Milton Keynes, England.",
				213 => "On this sol in 195: Viking Orbiter 1 frame 35A72 imaged Cydonia Face and Pyramids.",
				215 => "On this sol in 147: Henri Perrotin and Louis Thollon confirmed the existence of the canali.",
				219 => "On this sol in 193: Contact with Mariner 9 was lost after 338 sols in Mars orbit.",
				220 => "On this sol in 188: The first \"Symposium on the Exploration of Mars\" opened in Denver, Colorado. This was the first conference devoted to the exploration of Mars via spacecraft.",
				222 => "On this sol in 195: Viking Orbiter 1 frames 43A01 through 43A04 imaged Deuteronilus Crater Pyramid.",
				225 => "On this sol in 195: Viking 2 entered Mars orbit.",
				228 => "On this sol in 183: I. M. Levitt`s Earth-Mars Clock debuted in New York, New York.",
				232 => "On this sol in 176: Edgar Rice Burroughs began writing \"The Invisible Men of Mars,\" part 4 of a new Mars series.",
				233 => "On this sol in 136: A meteorite was observed to fall near Shergotty, India. In the 1980s, the Shergotty meteorite was identified as having originated on Mars.",
				234 => "On this sol in 152: \"The observations of 1894 have made it practically certain that the so-called `canals` of Mars are real, whatever may be their explanation,\" Princeton astronomer Charles A. Young declared in his article for \"Cosmopolitan,\" \"Mr. Lowell`s Theory of Mars\" (date approximate).\nOn this sol in 158: In a letter to \"The New York Times,\" Nikola Tesla declared, \"I can easily bridge the gulf which separates us from Mars.\"\nOn this sol in 162: Edgar Rice Burroughs began writing \"The Gods of Mars.\"",
				236 => "On this sol in 188: Mars 1 passed Mars three months after contact was lost.\nOn this sol in 206: \"Workshop on Early Mars\" opened in Houston, Texas.",
				238 => "On this sol in 175: Edgar Rice Burroughs` \"The Synthetic Men of Mars\" was published in \"Argosy.\"",
				239 => "On this sol in 153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 13th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				241 => "On this sol in 185: Isaac Asimov`s \"I`m in Marsport without Hilda\" appeared in \"Venture Science Fiction\" (date approximate).",
				242 => "On this sol in 212: The Eleventh International Mars Society Convention opened in Boulder, Colorado.",
				244 => "On this sol in 214: The Third International Conference on Early Mars opened in Lake Tahoe, Nevada.",
				246 => "On this sol in 205: The \"Mars Together\" conference opened in Palo Alto, California.\nOn this sol in 208: A meteorite, designated Sayh al Uhaymir 094, was discovered near Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.\nOn this sol in 211: The Fourth International Conference on Mars Polar Science and Exploration opened in Davos, Switzerland.",
				247 => "On this sol in 181: Ray Bradbury`s \"The Martian Chronicles\" was published (date approximate).",
				248 => "On this sol in 195: Viking Orbiter 1 frame 70A13 imaged the Cydonia Face and Pyramids.",
				249 => "On this sol in 213: The Tenth Australian Mars Exploration Conference opened in Melbourne, Victoria.",
				251 => "On this sol in 195: Viking Lander 2 landed in Utopia Planitia.",
				252 => "On this sol in 191: Mariner 6 was launched.",
				253 => "On this sol in 166: Edgar Rice Burroughs` \"The Chessmen of Mars\" began serialization in \"Argosy All-Story Weekly.\"",
				254 => "On this sol in 196: Contact with Viking Orbiter 2 was lost after 697 sols in Mars orbit.",
				257 => "On this sol in 211: The Sixth Australian Mars Exploration Conference opened in Melbourne, Victoria.",
				258 => "On this sol in 215: The Second Humans to Mars Summit opened in Washington, DC.",
				260 => "On this sol in 153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 14th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this sol in 182: Leigh Brackett`s \"The Last Days of Shandakor\" appeared in \"Startling Stories\" (date approximate).",
				264 => "On this sol in 211: The Sixth European Mars Convention opened in Paris, France.",
				266 => "On this sol in 182: Ray Bradbury`s \"The Wilderness\" was published in \"The Philadelphia Enquirer.\"\nOn this sol in 195: Viking Orbiter 1 frame 86A10 imaged the Utopia Faces.\nOn this sol in 204: Contact with Mars Observer was lost enroute to Mars.\nOn this sol in 208: Thomas W. Cronin`s \"As it Is on Mars\" was published.",
				269 => "On this sol in 202: NASA`s \"Report of the 90-Day Study on Human Exploration of the Moon and Mars\" estimated the cost of a manned Mars program at $450 billion. Political support for manned Mars missions subsequently collapsed.\nOn this sol in 204: Mars Observer passed Mars three days after contact was lost.",
				272 => "On this sol in 206: Kim Stanley Robinson`s \"Blue Mars\" was published by Spectra.\nOn this sol in 211: The motion picture \"Fascisti su Marte\" was released.",
				274 => "On this sol in 176: Edgar Rice Burroughs` \"John Carter and the Giant of Mars\" appeared in \"Amazing Stories\" (date approximate). The story was later included in the anthology \"John Carter of Mars.\"",
				276 => "On this sol in 204: Sailor Mars first appeared in the manga \"Run Run.\"\nOn this sol in 213: The Thirteenth International Mars Society Convention opened in Dayton, Ohio.",
				278 => "On this sol in 188: Philip K. Dick`s \"All We Marsmen\" began serialization in \"Worlds of Tomorrow\" (date approximate).",
				279 => "On this sol in 181: The motion picture \"Rocketship X-M\" was released.\nOn this sol in 191: Mariner 7 was launched.",
				281 => "On this sol in 191: Mars 1969A failed during launch.",
				282 => "On this sol in 209: Approximate time setting for \"February 2003: Interim\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				286 => "On this sol in 195: \"Today we have touched Mars. There is life on Mars and it is us--extensions of our eyes in all directions, extensions of our mind, extensions of our heart and soul have touched Mars today. That`s the message to look for there: We are on Mars. We are the Martians!\" --Ray Bradbury, speaking at \"The Search for Life in the Solar System\" symposium in Pasadena, California.",
				287 => "On this sol in 153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 15th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this sol in 180: Marvin the Martian first appeared in animated short film \"Haredevil Hare.\"\nOn this sol in 184: Robert A. Heinlein`s \"Double Star\" began serialization in \"Astounding Science Fiction\" (date approximate).",
				294 => "On this sol in 152: Percival Lowell`s \"Mars\" was published (date approximate).\nOn this sol in 183: The motion picture \"Mars and Beyond\" was released.",
				295 => "On this sol in 194: Lin Carter`s \"The Valley Where Time Stood Still\" was published by DAW (date approximate).",
				299 => "On this sol in 191: Mars 1969B failed during launch.",
				300 => "On this sol in 206: NASA issued \"Mars Design Reference Mission 3.0\" (date approximate). William K. Hartmann`s \"Mars Underground\" was published (date approximate).",
				301 => "On this sol in 153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 16th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				303 => "On this sol in 206: Mars Pathfinder landed in Ares Vallis. The spacecraft was later renamed the Carl Sagan Memorial Station.\nOn this sol in 208: Mars Odyssey was launched.",
				304 => "On this sol in 189: Mariner 4 achieved the first flyby of Mars, transmitted 22 images of a cratered surface.\nOn this sol in 212: The Eighth European Mars Convention opened in Antwerp, Belgium.",
				305 => "On this sol in 186: The motion picture \"The Angry Red Planet\" was released.\nOn this sol in 204: Kim Stanley Robinson`s \"Red Mars\" was published by Spectra.\nOn this sol in 206: Sojourner, the first roving vehicle on Mars, deployed from the Carl Sagan Memorial Station.",
				309 => "On this sol in 209: Thomas W. Cronin`s \"Give Us This Mars\" was published.",
				310 => "On this sol in 197: Contact with Viking Orbiter 1 was lost after 1,469 sols in Mars orbit.",
				311 => "On this sol in 132: Angelo Secchi described \"a large, triangular patch.\" Then known as the Hourglass Sea, Secchi instead called it the Canale Atlantico, the first use of the word canale applied to Mars. The feature was later named Syrtis Major by Giovanni Schiaparelli.",
				315 => "On this sol in 212: Contact with Phoenix lander was lost.",
				316 => "On this sol in 214: The Fifteenth International Mars Society Convention opened in Pasadena California.",
				319 => "On this sol in 202: The Eighth International Conference on Mars opened in Tucson, Arizona.\nOn this sol in 214: Mars Science Laboratory \"Curiosity\" landed in Aeolis Palus (\"Bradbury Landing\") in Gale Crater.",
				326 => "On this sol in 189: Zond 2 passed Mars four months after contact was lost.",
				332 => "On this sol in 176: Edgar Rice Burroughs` \"The City of Mummies\" appeared in \"Amazing Stories\" (date approximate). The story was later included in the anthology \"Llana of Gathol.\"",
				334 => "On this sol in 199: Lin Carter`s \"Down to a Sunless Sea\" was published by DAW (date approximate).",
				335 => "On this sol in 153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 17th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this sol in 188: The television series \"My Favorite Martian\" debuted.\nOn this sol in 205: Sailor Mars first appeared in \"An Uncharmed Life\" episode of the animated television series \"Sailor Moon\" (USA air date).",
				336 => "On this sol in 208: The First Australian Mars Exploration Conference opened in Melbourne, Victoria.",
				339 => "On this sol in 209: Approximate time setting for \"April 2003: The Musicians\" in Ray Bradbury`s \"The Martian Chronicles.\"\nOn this sol in 215: The Eighth International Conference on Mars opened in Pasadena, California.",
				341 => "On this sol in 193: Lin Carter`s \"The Man Who Loved Mars\" was published (date approximate).\nOn this sol in 213: US President Obama signed the NASA Authorization Act of 2010, which authorized a three-year funding commitment for continued development of the Orion spacecraft and the development of a new heavy-lift Space Launch System to support eventual manned lunar and interplanetary missions; however, the pace of these programs was slowed considerably in comparison to the canceled Constellation program.",
				347 => "On this sol in 177: Edgar Rice Burroughs` \"The Skeleton Men of Jupiter\" appeared in \"Amazing Stories\" (date approximate). Later included in the anthology \"John Carter of Mars.\"",
				351 => "On this sol in 158: The Martians launched the first of ten cylinders toward Earth, beginning the War of the Worlds.\nOn this sol in 213: The Tenth European Mars Convention opened in Warsaw, Poland.",
				352 => "On this sol in 158: During the War of the Worlds, the Martians launched the second of ten cylinders toward Earth.\nOn this sol in 210: Thomas W. Cronin`s \"Glory Be to Mars\" was published.",
				353 => "On this sol in 158: During the War of the Worlds, the Martians launched the third of ten cylinders toward Earth.",
				354 => "On this sol in 158: During the War of the Worlds, the Martians launched the fourth of ten cylinders toward Earth.",
				355 => "On this sol in 158: During the War of the Worlds, the Martians launched the fifth of ten cylinders toward Earth.",
				356 => "On this sol in 152: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the first of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this sol in 158: During the War of the Worlds, the Martians launched the sixth of ten cylinders toward Earth.\nOn this sol in 189: \"I say let`s do it quickly and establish a foothold on a new planet while we still have one left to take off from.\" --Wernher von Braun, \"Manned Mars Landing,\" \"Aviation Week & Space Technology.\"\nOn this sol in 208: Approximate time setting for \"June 2001: And the Moon Be Still as Bright\" in Ray Bradbury`s \"The Martian Chronicles.\"\nOn this sol in 214: The Twelfth Australian Mars Exploration Conference opened in Canberra, ACT.",
				357 => "On this sol in 158: During the War of the Worlds, the Martians launched the seventh of ten cylinders toward Earth.",
				358 => "On this sol in 158: During the War of the Worlds, the Martians launched the eighth of ten cylinders toward Earth.\nOn this sol in 187: NASA`s Office of Manned Space Flight published its \"Long Range Plan,\" which projected the launch of a manned Mars flyby mission in 192 and a manned Mars landing mission in 194.",
				359 => "On this sol in 158: During the War of the Worlds, the Martians launched the ninth of ten cylinders toward Earth.",
				360 => "On this sol in 158: During the War of the Worlds, the Martians launched the last of ten cylinders toward Earth.\nOn this sol in 207: The Fifth International Conference on Mars opened in Pasadena, California.",
				363 => "On this sol in 192: Mariner 8 failed during launch.\nOn this sol in 215: The Seventeenth International Mars Society Convention opened in League City, Texas.",
				364 => "On this sol in 190: NASA Manned Spacecraft Center issued \"Request for Proposal No. BG721-28-7-528P, Planetary Surface Sample Return Probe Study for Manned Mars/Venus Reconnaissance/Retrieval Missions\" in the 1975-1982 time frame. Rep. Joseph Karth, acting chairman of the House Subcommittee on NASA Oversight, who had been fighting an uphill battle to preserve Project Voyager funding, later expressed his exasperation, for the move cast the program of Saturn V-launched unmanned orbiters and landers in the role of a foot in the door for manned follow-on missions to the planets: \"Very bluntly, a manned mission to Mars or Venus by 1975 or 1977 [194 or 195] is now and always has been out of the question, and anyone who persists in this kind of misallocation of resources at this time will be stopped.\"",
				365 => "On this sol in 192: Kosmos 419 was launched, but failed to leave Earth orbit.",
				367 => "On this sol in 153: H. G. Wells` \"The War of the Worlds\" was published as a novella (date approximate).",
				368 => "On this sol in 188: Roger Zelazny`s \"A Rose for Ecclesiastes\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).\nOn this sol in 199: \"The Case For Mars II\" conference opened in Boulder, Colorado.",
				369 => "On this sol in 195: Gordon Eklund and Gregory Benford`s \"Hellas is Florida\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).",
				371 => "On this sol in 206: Mars Global Surveyor entered Mars orbit.",
				372 => "Autumnal Equinox (nominal date).",
				373 => "On this sol in 152: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the second of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this sol in 207: Approximate time setting for \"August 1999: The Summer Night\" and \"August 1999: The Earth Men\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				374 => "On this sol in 192: Mars 2 was launched.",
				377 => "On this sol in 190: U.S. Congress deleted funding for Voyager, a program of Saturn V-launched unmanned orbiters and landers, then scheduled for its first missions in 1973. The program was later down-scoped and renamed \"Viking.\"\nOn this sol in 200: The motion picture \"Invaders From Mars\" was released.",
				381 => "On this sol in 208: A meteorite, designated Sayh al Uhaymir 060, was discovered near Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.",
				382 => "On this sol in 153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 18th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this sol in 158: During the War of the Worlds, the first Martian cylinder landed on Horsell Common, east of Horsell.The Deputation advanced upon the pit, waving a white flag. There was a flash of light, an invisible ray of heat flashed from man to man, and each burst into flame, killing about 40 people.\nOn this sol in 207: \"The Second International Mars Society Convention\" opened in Boulder, Colorado.\nOn this sol in 210: Approximate time setting for \"April 2005: Usher II\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				383 => "On this sol in 158: During the War of the Worlds, the second Martian cylinder landed on the Addlestone Golf Links. A battalion of the Cardigan Regiment rushed the Horsell pit in skirmish order and was annihilated by the Heat-Ray. The fighting-machine then destroyed Woking.\nOn this sol in 192: Mars 3 was launched.",
				384 => "On this sol in 158: During the War of the Worlds, the third Martian cylinder landed north of Pyrford, completing the Surrey Triangle. Five Martian fighting-machines advanced down the Wey River to the confluence with the Thames. Royal Army batteries engaged the Martians, destroying one fighting-machine, but Weybridge and Chertsey were destroyed by Heat-Ray. Later, the St. George's Hill battery damaged one fighting-machine, but was then destroyed. Seven Martian fighting-machines fanned out along a curved line between St. George's Hill, Weybridge, and the village of Send. The Martians discharged Black Smoke across the valley of the Thames, advancing through Street Cobham and Ditton. Richmond, Kingston, and Wimbledon were destroyed.\nOn this sol in 214: The Twelfth European Mars Convention opened in Munich, Germany.",
				385 => "On this sol in 158: During the War of the Worlds, the fourth Martian cylinder landed in Bushey Park, beginning the West End Triangle. The Martians advanced in a line from Hanwell in the north to Coombe and Malden in the south. Organized resistance by the British forces collapsed. The Martians went to and fro over the North Downs between Guildford and Maidstone, using the Black Smoke to eliminate any artillery batteries located there. Police organization in London broke down. The railway system collapsed.\nOn this sol in 192: Mariner 9 was launched.",
				386 => "On this sol in 158: During the War of the Worlds, the fifth Martian cylinder landed in Sheen, and the sixth Martian cylinder landed in Wimbledon, completing the West End Triangle.",
				387 => "On this sol in 158: During the War of the Worlds, the seventh Martian cylinder landed in Primrose Hill, where the invaders established their new headquarters. \"HMS Thunder Child\" made a suicide run at three fighting-machines the mouth of the Blackwater to cover the escape of passenger vessels. Two fighting-machines were destroyed. \"Thunder Child\" was also destroyed.",
				388 => "On this sol in 158: During the War of the Worlds, the eighth Martian cylinder landed (unreported). A fighting-machine destroyed Leatherhead, with every soul in it.",
				389 => "On this sol in 158: During the War of the Worlds, the ninth Martian cylinder landed (unreported). The Martians vacated the Sheen cylinder except for one fighting-machine and one handling-machine.",
				390 => "On this sol in 158: During the War of the Worlds, the last of ten Martian cylinder landed (unreported).",
				391 => "On this sol in 153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 19th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this sol in 165: Ray Bradbury was born, author of \"The Martian Chronicles.\"\nOn this sol in 169: Edgar Rice Burroughs began dictating \"A Fighting Man of Mars\" on Ediphone.\nOn this sol in 215: The Fourteenth European Mars Convention opened in Podzamcze, Poland.",
				392 => "On this sol in 207: A meteorite, designated Dar al Gani 975, was discovered near Dar al Gani, Libya. The meteorite was later identified as having originated on Mars.",
				395 => "On this sol in 206: Contact with the Carl Sagan Memorial Station (Mars Pathfinder) was lost after 93 sols in Ares Vallis.",
				399 => "On this sol in 195: John Varley`s \"In the Hall of the Martian Kings\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).\nOn this sol in 209: Approximate time setting for \"June 2003: Way in the Middle of the Air\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				400 => "On this sol in 206: The motion picture \"RocketMan\" was released.\nOn this sol in 209: Mars Express was launched.",
				402 => "On this sol in 158: The last Martians succumbed to terrestrial disease, ending the War of the Worlds.\nOn this sol in 201: Soviet President Mikhail Gorbachev called for a joint unmanned Mars mission with the USA.",
				404 => "On this sol in 158: Robert A. Heinlein was born, author of \"Red Planet,\" \"Podkayne of Mars,\" and \"Stranger in a Strange Land.\"\nOn this sol in 159: Nikola Tesla discussed communication with Mars in an article in \"The New York Times.\"\nOn this sol in 191: Mariner 6 passed Mars, returned 26 near encounter images.",
				407 => "On this sol in 214: Mars Atmosphere and Volatile EvolutioN Mission (MAVEN) entered Mars orbit",
				408 => "On this sol in 209: Mars Exploration Rover Spirit was launched.",
				409 => "On this sol in 191: Mariner 7 passed Mars, returned 33 near encounter images.\nOn this sol in 214: Mars Orbiter Mission (MOM), also called Mangalyaan, entered Mars orbit.",
				410 => "On this sol in 201: The Planetary Society`s \"Mars Declaration\" was published in \"The Washington Post.\"",
				416 => "On this sol in 202: The motion picture \"Martians Go Home\" was released.\nOn this sol in 208: Approximate time setting for \"August 2001: The Settlers\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				417 => "On this sol in 153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 20th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				420 => "On this sol in 119: Giovanni Schiaparelli was born, developed first detailed maps of Mars and originated the system of areographic nomenclature in use today.\nOn this sol in 200: The \"NASA Mars Conference\" opened in Washington, DC.",
				421 => "On this sol in 176: Edgar Rice Burroughs` \"The Black Pirates of Barsoom\" appeared in \"Amazing Stories\" (date approximate). The story was later included in the anthology \"Llana of Gathol.\"\nOn this sol in 208: \"The Infinite Worlds of H.G. Wells\" episode \"The Crystal Egg\" aired.",
				422 => "On this sol in 203: US President George H. W. Bush issued National Space Policy Directive 6, \"Space Exploration Initiative,\" calling for human expeditions to Mars.",
				423 => "On this sol in 207: Two meteorites, designated Los Angeles 001 and 002, were discovered in the Mojave Desert near Los Angeles, California. The meteorites were later identified as having originated on Mars.\nOn this sol in 210: \"Star Trek: Enterprise\" episode \"Terra Prime\" aired.",
				424 => "On this sol in 196: The Second International Colloquium on Mars opened in Pasadena, California.",
				428 => "On this sol in 202: Voicing his opposition to the Bush Administration`s Space Exploration Initiative, Senator Albert Gore, Jr. told his fellow legislators, \"Before discussing a mission to Mars, the Administration needs a mission to reality.\"",
				429 => "On this sol in 151: The Lowell Observatory began its study of Mars. Eighteen months later, Percival Lowell was published the observations of the 151 opposition in a popular book, Mars.",
				431 => "On this sol in 091: William Herschel calculated a rotational period for Mars of 24 hours, 39 minutes, and 21.67 seconds. He also confirmed that the north polar spot was eccentric to the pole.",
				434 => "On this sol in 209: Mars Exploration Rover Opportunity was launched.",
				435 => "On this sol in 153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 21st of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this sol in 191: A NASA report to US President Richard Nixon`s Space Task Group, which was chartered to explore options for space efforts beyond the Apollo program, stated, \"Manned expeditions to Mars could begin as early as 1981.\"",
				436 => "On this sol in 202: George Bush became the first US President to set a target date for sending humans to Mars: \"I believe that before America celebrates the 50th anniversary of its landing on the Moon, the American flag should be planted on Mars.\"",
				437 => "On this sol in 208: \"The Fourth International Mars Society Convention\" opened at Stanford University, California.",
				439 => "On this sol in 188: \"The Outer Limits\" episode \"Controlled Experiment\" aired.",
				440 => "On this sol in 015: Two years after making his first drawing of Mars, Neapolitan lawyer and amateur astronomer Francesco Fontana produced a second drawing, which like the first, featured a dark spot, which has been attributed to an optical defect in his telescope.",
				445 => "On this sol in 210: The motion picture \"Crimson Force\" was released.",
				447 => "On this sol in 176: Edgar Rice Burroughs` \"The Black Pirates of Barsoom\" was published in Amazing Stories.\nOn this sol in 198: Contact with the Thomas A. Mutch Memorial Station (Viking Lander 1) was lost after 2,244 sols in Chryse Planitia.\\nOn this sol in 209: The Sixth International Conference on Mars opened in Pasadena, California.",
				449 => "On this sol in 158: I. M. Levitt was born, inventor of a Martian calendar and the first Earth-Mars mechanical date-time computer.\nOn this sol in 191: The report by US President Richard Nixon`s Space Task Group, \"The Post-Apollo Space Program: Directions for the Future,\" stated, \"We conclude that NASA has the demonstrated organizational competence and technology base, by virtue of the Apollo success and other achievements, to carry out a successful program to land man on Mars within 15 years.\" Nevertheless, the report backed away from an early manned landing on Mars, recommending that the focus for the next decades in space should be on the development of hardware and systems that would ultimately support a manned mission to Mars at the close of the 20th century.",
				450 => "On this sol in 201: Phobos 1 was launched.",
				451 => "On this sol in 141: Edgar Rice Burroughs was born, author of the \"Barsoom\" series.",
				455 => "On this sol in 010: Christiaan Huygens was born, drew the first identifiable Martian surface feature.\nOn this sol in 201: Phobos 2 was launched.",
				457 => "On this sol in 202: The motion picture \"Total Recall\" was released.",
				458 => "On this sol in 180: Robert A. Heinlein`s \"Red Planet\" was published by Charles Scribner`s Sons.",
				459 => "On this sol in 202: \"The Case For Mars IV\" conference opened in Boulder, Colorado.",
				460 => "On this sol in 170: Edgar Rice Burroughs` \"A Fighting Man of Mars\" began serialization in \"Blue Book\" (date approximate).",
				464 => "On this sol in 214: The motion picture \"Master Race from Mars\" was released.",
				469 => "On this sol in 182: Isaac Asimov`s \"The Martian Way\" appeared in \"Galaxy Science Fiction\" (date approximate).\nOn this sol in 210: The motion picture \"The War of the Worlds\" was released.",
				471 => "On this sol in 125: Omsby MacKnight Mitchel observed a white patch detached from the south polar cap of Mars. The feature became known as the Mountains of Mitchel. Actually it is a depression.",
				472 => "On this sol in 208: The First European Mars Convention opened in Paris, France.",
				473 => "On this sol in 142: Asaph Hall gave up a search for Martian moons. The following night, having resumed his search at the insistence of his wife, Angelina, he detected a faint object near Mars, which he later named Deimos.",
				474 => "On this sol in 142: \"The New York Times\" asked, \"Is Mars inhabited?\" in an editorial. As the best opposition since 1798 approached, questions in the popular mind came to the fore and the possibility of life on Mars was discussed in the press.",
				478 => "On this sol in 142: Asaph Hall discovered Phobos.\nOn this sol in 215: The Orion spacecraft, designed for manned interplanetary missions, was tested successfully in Earth orbit on an unmanned flight.",
				479 => "On this sol in 193: Mars 4 was launched.",
				480 => "On this sol in 142: Asaph Hall announced the discovery of Mars` two moons. At the suggestion of Henry Madan, the Science Master of Eton, England, Hall named the moons Phobos and Deimos.",
				481 => "On this sol in 160: An unfortunate dog in Nakhla, Egypt was struck by part of a meteorite and killed. In the 1980s, the Nakhla meteorite was identified as having originated on Mars.\nOn this sol in 183: Alfred Coppel`s \"Mars Is Ours\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).",
				482 => "On this sol in 176: Edgar Rice Burroughs` \"The Yellow Men of Mars\" appeared in \"Amazing Stories\" (date approximate). The story was later included in the anthology \"Llana of Gathol.\"\nOn this sol in 184: A dust storm began with a bright cloud over the Hellas-Noachis region that spread to engulf the whole planet by mid-September.",
				483 => "On this sol in 193: Mars 5 was launched.\nOn this sol in 209: In perihelic solar inferior conjunction, Earth made its closest approach to Mars in 32,000 Martian years, coming within 55.8 million kilometers.",
				484 => "On this sol in 160: Writing down his daydreams on the backs of old letterheads of previously failed businesses, Edgar Rice Burroughs used free time at his office to begin \"A Princess of Mars\" (date approximate).\nOn this sol in 203: Sailor Mars first appeared in \"An Uncharmed Life\" episode of the animated television series \"Sailor Moon\" (Japan air date).",
				485 => "Perihelion (nominal date).\nOn this sol in 207: Two meteorites, designated Sayh al Uhaymir 005 and 008, were discovered near Sayh al Uhaymir, Oman. The meteorites were later identified as having originated on Mars.",
				487 => "On this sol in 209: The Third Australian Mars Exploration Conference opened in Perth, Western Australia.",
				490 => "On this sol in 142: Henry Draper of New York and Edward Singleton Holden of the U.S. Naval Observatory in Washington claimed to have jointly discovered a third moon of Mars at Draper`s private observatory at Hastings-on-the-Hudson. This discovery proved to be false; in fact, the proposed moon did not even obey Kepler`s laws.\nOn this sol in 151: The British scientific journal \"Nature\" reported that the Lick and Nice Observatories had seen a great light on Mars, which was later speculated to have been the casting of the huge gun used to launch the War of the Worlds.",
				492 => "On this sol in 207: Contact with Mars Polar Lander was lost prior to landing on Mars.\nOn this sol in 221: Approximate time setting for \"April 2026: The Long Years\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				493 => "On this sol in 185: A few months before it was transformed into a space agency by the National Aeronautics and Space Act, a report by the National Advisory Committee on Aeronautics projected that a manned Mars mission might be undertaken in 1977.",
				494 => "On this sol in 193: Mars 6 was launched.",
				495 => "On this sol in 208: Mars Odyssey entered Mars orbit.",
				497 => "On this sol in 193: Mars 7 was launched.",
				501 => "Storm season begins.\nOn this sol in 210: Approximate time setting for \"August 2005: The Old Ones\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				504 => "On this sol in 142: Giovanni Schiaparelli began a detailed study of Mars for the purpose of drawing a new and accurate map of the planet.",
				505 => "On this sol in 201: Contact with Phobos 1 was lost en route to Mars.",
				507 => "On this sol in 142: Giovanni Schiaparelli observed the canale Cyclops.",
				510 => "On this sol in 167: The motion picture \"Aelita: Queen of Mars\" was released.",
				511 => "On this sol in 210: The Eighth International Mars Society Convention opened in Boulder, Colorado. Mars Reconnaissance Orbiter was launched.",
				512 => "On this sol in 209: The Third European Mars Convention opened in Bremen, Germany.",
				513 => "On this sol in 162: Orson Welles was born, leader of the Mercury Theater of the Air, which performed the \"Panic Broadcast.\"\nOn this sol in 210: \"The Sixth International Mars Society Convention\" opened in Eugene, Oregon.",
				514 => "Winter Solstice (nominal date).\nOn this sol in 142: Giovanni Schiaparelli observed the canale Ambrosia.",
				515 => "On this sol in 188: Philip K. Dick`s \"Martian Time-Slip\" was published (date approximate).",
				518 => "On this sol in 210: The Fifth Australian Mars Exploration Conference opened in Canberra, ACT.",
				519 => "On this sol in 186: Arthur C. Clarke`s \"Crime on Mars,\" later titled \"The Trouble with Time,\" was published in \"Ellery Queen\" (date approximate).",
				520 => "On this sol in 141: Earl C. Slipher was born, observed Mars extensively.\nOn this sol in 211: The Seventh International Conference on Mars opened in Pasadena, California.",
				523 => "On this sol in 211: The Seventh Australian Mars Exploration Conference opened in Perth, Western Australia.",
				525 => "On this sol in 142: Giovanni Schiaparelli observed the canali Ganges and Phison.",
				526 => "On this sol in 165: Edgar Rice Burroughs began writing \"The Chessmen of Mars.\"",
				527 => "On this sol in 130: Percival Lowell was born, developed detailed maps of Mars and believed in an advanced Martian civilization.\nOn this sol in 196: Robert F. Young`s \"The First Mars Mission\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).",
				529 => "On this sol in 203: Michael Cassutt`s \"The Last Mars Trip\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).\nOn this sol in 209: The Third International Conference on Mars Polar Science and Exploration opened in Lake Louise, Alberta.",
				530 => "On this sol in 166: Edgar Rice Burroughs` \"The Chessmen of Mars\" was published by McClurg.\nOn this sol in 170: Lin Carter was born, author of \"The Man Who Loved Mars.\"",
				531 => "On this sol in 142: Giovanni Schiaparelli observed Mare Erythraeum and Noachis to be obscured by clouds.\nOn this sol in 210: Approximate time setting for \"September 2005: The Martian\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				533 => "On this sol in 199: A meteorite, designated ALH 84001, was discovered in the Allan Hills, Antarctica. In 1993, the meteorite was identified as having originated on Mars. In 1996, a team of scientists announced evidence on fossilized Martian life in the meteorite.",
				534 => "On this sol in 208: Approximate time setting for \"December 2001: The Green Morning\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				537 => "On this sol in 092: William Herschel observed the south polar cap of Mars. \"I am inclined to think that the white spot has some little revolution.... It is rather probable that the real pole, though within the spot, may lie near the circumference of it, or one-third of its diameter from one of the sides. A few days more will show it, as I shall now fix my particular attention on it.\"",
				540 => "On this sol in 160: \"Martians Build Two Immense Canals in Two Years\" reported in \"The New York Times.\"\nOn this sol in 176: Edgar Rice Burroughs` \"The Invisible Men of Mars\" appeared in Amazing Stories (date approximate). The story was later included in the anthology Llana of Gathol.",
				541 => "On this sol in 142: Giovanni Schiaparelli observed the canale Eunostos.",
				542 => "On this sol in 207: A meteorite, designated Dhofar 019, was discovered near Dhofar, Oman. The meteorite was later identified as having originated on Mars.",
				545 => "On this sol in 211: Phoenix lander was launched.",
				547 => "On this sol in 173: ERB Inc released \"The Swords of Mars.\"",
				548 => "On this sol in 192: Mariner 9 became the first spacecraft to orbit Mars.",
				549 => "On this sol in 109: A meteorite was observed to fall near Chassigny, France. In the 1980s, the Chassigny meteorite was identified as having originated on Mars. It was the first Martian meteorite to be discovered.\nOn this sol in 194: Viking 1 was launched.",
				554 => "On this sol in 212: NASA issued \"Human Exploration of Mars Design Reference Architecture 5.0\" (date approximate).",
				558 => "On this sol in 189: Philip K. Dick`s \"We Can Remember It for You Wholesale\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate). It was the basis of of the 1990 motion picture \"Total Recall.\"",
				559 => "On this sol in 207: A meteorite, designated GRV 99027, was discovered in the Grove Mountains, Antarctica. The meteorite was later identified as having originated on Mars.",
				561 => "On this sol in 192: Mariner 9 returned the first image of Deimos. Mars 2 Orbiter entered Mars orbit. Mars 2 Lander impacted on Mars.",
				564 => "On this sol in 176: Edgar Rice Burroughs began writing \"The Skeleton Men of Jupiter,\" the first of a planned new John Carter series.\nOn this sol in 192: Mariner 9 returned the first image of Phobos.",
				565 => "On this sol in 209: A meteorite, designated Sayh al Uhaymir 125, was discovered near Dar al Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.",
				566 => "On this sol in 192: Mars 3 Orbiter entered Mars orbit. Contact with Mars 2 Lander was lost shortly after landing. No useful data was returned.",
				567 => "On this sol in 178: The motion picture \"The Purple Monster Strikes\" was released.",
				568 => "On this sol in 197: \"The Case For Mars I\" conference opened in Boulder, Colorado.",
				569 => "On this sol in 212: The Ninth Australian Mars Exploration Conference opened in Adelaide, South Australia.",
				571 => "On this sol in 191: An article in \"Time\" magazine speculated that the Charles Manson \"family\" might have been a Martian nest patterned on Robert A. Heinlein`s \"Stranger in a Strange Land.\" The \"family\" had murdered actress Sharon Tate and four others in Los Angeles.\nOn this sol in 194: Viking 2 was launched.",
				573 => "On this sol in 123: Camille Flammarion was born, developed detailed maps of Mars and believed in an advanced civilization on that planet.\nOn this sol in 211: The Tenth International Mars Society Convention opened in Los Angeles, California.",
				581 => "On this sol in 207: Approximate time setting for \"March 2000: The Taxpayer\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				582 => "On this sol in 208: A meteorite, designated Sayh al Uhaymir 090, was discovered near Dar al Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.",
				583 => "On this sol in 212: The Twelfth International Mars Society Convention opened in College Park, Maryland.",
				585 => "On this sol in 214: The First Humans to Mars Summit opened in Washington, DC.",
				586 => "On this sol in 182: Lester Del Rey and Erik Van Lhin`s \"Police Your Planet\" began serialization in \"Science Fiction Adventures\" (date approximate).",
				590 => "On this sol in 143: Discussing the canali in a letter to Nathaniel Green, Giovanni Schiaparelli declared, \"It is [as] impossible to doubt their existence as that of the Rhine on the surface of the Earth.\"\nOn this sol in 207: The motion picture \"Mission to Mars\" was released.\nOn this sol in 209: A meteorite, designated MIL 03346, was discovered in the Miller Range of the Transantarctic Mountains. The meteorite was later identified as having originated on Mars.\nOn this sol in 210: Approximate time setting for \"November 2005: The Luggage Store,\" \"November 2005: The Off Season,\" and \"November 2005: The Watchers\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				593 => "On this sol in 210: The Fifth European Mars Convention opened in Swindon, England.",
				594 => "On this sol in 153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 22nd of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				595 => "On this sol in 208: Approximate time setting for \"February 2002: The Locusts\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				596 => "On this sol in 206: A meteorite, designated Dar al Gani 476, was discovered near Dar al Gani, Libya. The meteorite was later identified as having originated on Mars.",
				599 => "On this sol in 201: Soviet President Mikhail Gorbachev called for a joint manned Mars mission with the USA.",
				600 => "On this sol in 209: Mars Express entered orbit. Contact with Beagle 2 was lost prior to landing in Isidis Planitia.",
				602 => "On this sol in 206: A meteorite, designated Dar al Gani 876, was discovered near Dar al Gani, Libya. The meteorite was later identified as having originated on Mars.",
				603 => "On this sol in 193: The First International Colloquium on Mars opened in Pasadena, California.",
				604 => "On this sol in 143: Giovanni Schiaparelli discovered a small white patch in Tharsis and named it Nix Olympica. Nearly a century later, Mariner 9 imagery revealed this feature to be the largest mountain in the Solar System, and the feature was renamed Olympus Mons.",
				607 => "On this sol in 152: Charles A. Young of Princeton University discussed the question \"Is Mars Inhabited?\" in \"The Boston Herald.\"\nOn this sol in 153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 23rd of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n",
				610 => "On this sol in 209: Mars Exploration Rover Spirit landed in Gusev Crater. The spacecraft was later renamed the Columbia Memorial Station.\nOn this sol in 221: Approximate time setting for \"August 2026: There Will Come Soft Rains\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				611 => "On this sol in 207: Approximate time setting for \"April 2000: The Third Expedition\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				613 => "On this sol in 201: A meteorite, designated LEW 88516, was discovered at Lewis Cliff, Antarctica. The meteorite was later identified as having originated on Mars.\nOn this sol in 201: Percival Lowell reported \"Frost on Mars\" to \"The New York Times.\"\nOn this sol in 203: Mars Observer was launched.\nOn this sol in 213: A meteorite, designated Tissint, was observed to fall in Tata Province in the Guelmim-Es Semara region of Morocco. The meteorite was later identified as having originated on Mars.",
				614 => "On this sol in 136: H. G. Wells was born, author of \"The War of the Worlds.\"",
				616 => "On this sol in 209: Two meteorites, designated Sayh al Uhaymir 130 and 131, were discovered near Dar al Sayh al Uhaymir, Oman. The meteorites were later identified as having originated on Mars.",
				617 => "On this sol in 165: The motion picture \"A Message from Mars\" was released.\nOn this sol in 186: Mars 1960A, the first known Mars mission, failed during launch.\nOn this sol in 213: The Eleventh Australian Mars Exploration Conference opened in Perth, Western Australia.",
				618 => "On this sol in 161: Edgar Rice Burroughs drew a map of Barsoom at the request of his publisher.",
				619 => "On this sol in 210: Approximate time setting for \"December 2005: The Silent Towns\" in Ray Bradbury`s \"The Martian Chronicles.\"\nOn this sol in 211: The Seventh European Mars Convention opened in Delft, Netherlands.",
				620 => "On this sol in 188: The motion picture \"Robinson Crusoe on Mars\" was released.\nOn this sol in 209: US President George W. Bush announced plans to establish a lunar base to support human missions to Mars.",
				621 => "On this sol in 182: The motion picture \"Abbott and Costello Go to Mars\" was released.\nOn this sol in 186: Mars 1960B failed during launch.",
				626 => "On this sol in 216: The Third Humans to Mars Summit opened in Washington, DC.",
				629 => "On this sol in 174: Edgar Rice Burroughs began writing \"The Synthetic Men of Mars.\"\nOn this sol in 205: \"The Case for Mars VI\" conference opened in Boulder, Colorado.",
				630 => "On this sol in 209: Mars Exploration Rover Opportunity landed in Meridiani Planum.\nOn this sol in 213: The Fourteenth International Mars Society Convention opened in Dallas, Texas.",
				632 => "On this sol in 175: ERB Inc released \"The Synthetic Men of Mars.\"\nOn this sol in 192: Mariner 9 frame 4205-78 imaged Pyramids of Elysium.",
				635 => "On this sol in 195: Lin Carter`s \"The City Outside the World\" was published by Berkeley (date approximate).",
				636 => "On this sol in 153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 24th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				637 => "On this sol in 182: The motion picture \"Invaders From Mars\" was released.",
				640 => "Storm season ends.\nOn this sol in 000: Galileo Galilei discovered that, like the Moon, Mars had a phase.",
				643 => "On this sol in 026: Christiaan Huygens drew the first sketch of Mars, including a dark, triangular area later named Syrtis Major, the first Martian surface feature identified from Earth. Huygens used his own design of telescope, which was of much higher quality than that of his predecessors and allowed a magnification of 50 times.",
				646 => "On this sol in 026: Christiaan Huygens noted, \"The rotation of Mars, like that of the Earth, seems to have a period of 24 hours.\"\nOn this sol in 153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 25th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this sol in 201: Phobos 1 passed Mars five months after contact was lost.",
				647 => "On this sol in 185: The motion picture \"Quatermass and the Pit\" was released.",
				649 => "On this sol in 152: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the third of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this sol in 205: The National Science Foundation announced evidence for possible early life on Mars in Antarctic meteorite ALH84001.",
				650 => "On this sol in 201: Phobos 2 entered Mars orbit.",
				653 => "On this sol in 152: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the fourth of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this sol in 187: A meteorite was observed to fall near Zagami, Nigeria. In the 1980s, this meteorite was identified as having originated on Mars.",
				654 => "On this sol in 153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 26th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				657 => "On this sol in 212: The Ninth European Mars Convention opened in Bergamo, Italy.",
				659 => "On this sol in 153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 27th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				662 => "On this sol in 152: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the fifth of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				663 => "On this sol in 208: The motion picture \"Stranded\" was released.",
				664 => "On this sol in 152: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the sixth of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				665 => "On this sol in 153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 28th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				668 => "On this sol in 213: The Fifth International Conference on Mars Polar Science and Exploration opened in Fairbanks, Alaska.",
				_ => string.Empty
			};


			double MarsSolNum = doI + 1;
			mMonth = new List<int>([
				28, 56, 84, 112, 140, 167, 195, 223, 251, 279, 307, 334, 362, 390, 418, 446, 474, 501, 529, 557, 585, 613, 641, 669
				]).FindIndex(n => n >= MarsSolNum)
				+ 1;

			mMonthName = mMonth switch
			{
				1 => "Sagittarius",
				2 => "Dhanus",
				3 => "Capricornus",
				4 => "Makara",
				5 => "Aquarius",
				6 => "Kumbha",
				7 => "Pisces",
				8 => "Mina",
				9 => "Aries",
				10 => "Mesha",
				11 => "Taurus",
				12 => "Rishabha",
				13 => "Gemini",
				14 => "Mithuna",
				15 => "Cancer",
				16 => "Karka",
				17 => "Leo",
				18 => "Simha",
				19 => "Virgo",
				20 => "Kanya",
				21 => "Libra",
				22 => "Tula",
				23 => "Scorpius",
				24 => "Vrishika",
				_ => string.Empty
			};
		}

		static void SetEarthDateFromDays(double daysSince)
		{
			// get the fractional part, to do the time later
			double partialDay = daysSince - Math.Floor(daysSince);

			// Convert days to Gregorian date:
			var d = Math.Floor(daysSince) + 1;

			var sCD = Math.Floor(d / 146097);   // what 400 year span
			var doCD = Math.Floor(d - (sCD * 146097));

			var sC = 0;
			var doC = doCD;
			if (doCD != 0) sC = (int)Math.Floor((doCD - 1) / 36524);
			if (sC != 0) doC -= sC * 36524 + 1;
			var doIV = doC;

			int sIV;
			if (sC != 0)    // 1460 + 1461*24
			{
				sIV = (int)Math.Floor((doC + 1) / 1461);
				if (sIV != 0) doIV -= sIV * 1461 - 1;
			}
			else    // 1461*25
			{
				sIV = (int)Math.Floor(doC / 1461);
				if (sIV != 0) doIV -= sIV * 1461;
			}

			var sI = 0;
			var doI = doIV;
			if (sC != 0 && sIV == 0)    // four 365-day years in a row
			{
				sI = (int)Math.Floor(doIV / 365);
				if (sI != 0) doI -= sI * 365;
			}
			else    // normal leap year cycle
			{
				if (doI != 0) sI = (int)Math.Floor((doIV - 1) / 365);
				if (sI != 0) doI -= sI * 365 + 1;
			}

			earthYear = (int)(400 * sCD + 100 * sC + 4 * sIV + sI);
			var tmpDayOfYear = doI + 1;
			int earthMonth = 12;

			for (int i = 1; i < 12; i++)
			{
				var tmpDaysInMonth = eDaysInMonth[i];
				if (i == 2 && !IsEarthLeapYear(earthYear))
					tmpDaysInMonth -= 1;

				if (tmpDayOfYear > tmpDaysInMonth)
					tmpDayOfYear -= tmpDaysInMonth;
				else
				{
					earthMonth = i;
					break;
				}
			}

			double earthDay;

			earthDay = tmpDayOfYear;
			eDayOfWeek = (int)((Math.Floor(daysSince) % 7) + 1);

			eDayName = ((Math.Floor(daysSince) + 1) % 7) switch
			{
				1 => "Sunday",
				2 => "Monday",
				3 => "Tuesday",
				4 => "Wednesday",
				5 => "Thursday",
				6 => "Friday",
				0 => "Saturday",
				_ => string.Empty
			};

			// Put the result up:
			eYear = earthYear;
			eMonth = earthMonth;
			eDay = (int)earthDay;

			var tmpHour = partialDay * 24;
			var tmpMin = (tmpHour - Math.Floor(tmpHour)) * 60;
			var tmpSec = (tmpMin - Math.Floor(tmpMin)) * 60;
			var tmpMil = (tmpSec - Math.Floor(tmpSec)) * 1000;
			eHour = (int)Math.Floor(tmpHour);
			eMin = (int)tmpMin;
			eSec = (int)tmpSec;
			eMil = (int)tmpMil;

			if ((earthYear % 4 == 0 && earthYear % 100 != 0) || earthYear % 400 == 0)
				LeapDay = 1;
			else
				LeapDay = 0;

			if (doI + 1 < 60)
				LeapDay = 0;

			ThisDay = (doI + 1 - LeapDay) switch
			{
				1 => "On this day in 1898: H. G. Wells` \"The War of the Worlds\" was published as a novella (date approximate).\nOn this day in 1941: Edgar Rice Burroughs` \"John Carter and the Giant of Mars\" appeared in \"Amazing Stories\" (date approximate). The story was later included in the anthology \"John Carter of Mars.\"\nOn this day in 1977: Gordon Eklund and Gregory Benford`s \"Hellas is Florida\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).\nOn this day in 1995: Jerry Oltion`s \"The Great Martian Pyramid Hoax\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).\nOn this day in 1999: Approximate time setting for \"January 1999: Rocket Summer\" in Ray Bradbury`s \"The Martian Chronicles.\"\nOn this day in 2013: The motion picture \"Master Race from Mars\" was released.",
				2 => "On this day in 1920: Isaac Asimov was born, author of \"The Martian Way.\"",
				3 => "On this day in 1999: Mars Polar Lander was launched.",
				4 => "On this day in 2004: Mars Exploration Rover Spirit landed in Gusev Crater. The spacecraft was later renamed the Columbia Memorial Station.",
				7 => "On this day in 1921: Edgar Rice Burroughs began writing \"The Chessmen of Mars.\"\nOn this day in 1939: Edgar Rice Burroughs` \"The Synthetic Men of Mars\" was published in \"Argosy.\"",
				10 => "On this day in 1990: The Fourth International Conference on Mars opened in Tucson, Arizona.",
				11 => "On this day in 2004: Two meteorites, designated Sayh al Uhaymir 130 and 131, were discovered near Dar al Sayh al Uhaymir, Oman. The meteorites were later identified as having originated on Mars.",
				12 => "On this day in 1997: Marvin the Martian appeared in \"The Springfield Files\" episode of the animated television series \"The Simpsons.\"",
				13 => "On this day in 1964: \"The Outer Limits\" episode \"Controlled Experiment\" aired.\nOn this day in 1907: In a letter to \"The New York Times,\" Nikola Tesla declared, \"I can easily bridge the gulf which separates us from Mars.\"\nOn this day in 1980: A meteorite, designated EETA 79001, was discovered in the Elephant Moraine, Antarctica. The meteorite was later identified as having originated on Mars.",
				14 => "On this day in 1954: I. M. Levitt`s Earth-Mars Clock debuted in New York, New York.\nOn this day in 2004: US President George W. Bush announced plans to establish a lunar base to support human missions to Mars.",
				15 => "On this day in 1898: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 18th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this day in 1905: \"The New York Times\" reported that Nikola Tesla might use an oscillator to \"wake up\" Mars.\nOn this day in 1949: Robert A. Heinlein`s \"Red Planet\" was published by Charles Scribner`s Sons.\nOn this day in 1979: The Second International Colloquium on Mars opened in Pasadena, California.",
				16 => "On this day in 1950: Japanese astronomer Sadao Saeki saw a huge explosion on Mars which produced a mushroom cloud 1450 km in diameter \"like the terrific explosion of a volcano.\" No other people observed this explosion.",
				19 => "On this day in 1882: Giovanni Schiaparelli recorded the first instance of gemination.\"Great was my astonishment on January 19, when, on examining the Jamuna... I saw instead of its usual appearance two straight and equal parallel lines running between the Niliacus Lacus and Aurorae Sinus. At first I believed this to be the deception of a tired eye, or perhaps the effect of some kind of strabismus, but I soon convinced myself that the phenomenon was real.\"\nOn this day in 1970: An article in \"Time\" magazine speculated that the Charles Manson \"family\" might have been a Martian nest patterned on Robert A. Heinlein`s \"Stranger in a Strange Land.\" The \"family\" had murdered actress Sharon Tate and four others in Los Angeles.\nOn this day in 2002: A meteorite, designated Sayh al Uhaymir 090, was discovered near Dar al Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.",
				24 => "On this day in 2000: A meteorite, designated Dhofar 019, was discovered near Dhofar, Oman. The meteorite was later identified as having originated on Mars.",
				25 => "On this day in 1898: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 19th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this day in 1989: Phobos 1 passed Mars five months after contact was lost.\nOn this day in 2004: Mars Exploration Rover Opportunity landed in Meridiani Planum.",
				27 => "On this day in 1980: The first episode of the miniseries \"The Martian Chronicles\" aired.",
				28 => "On this day in 1980: The second episode of the miniseries \"The Martian Chronicles\" aired.",
				29 => "On this day in 1980: The third and final episode of the miniseries \"The Martian Chronicles\" aired.\nOn this day in 1989: Phobos 2 entered Mars orbit.",
				31 => "On this day in 1897: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the seventh of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				32 => "On this day in 1912: Edgar Rice Burroughs` \"Under the Moons of Mars\" began serialization in \"All-Story,\" later was published as a novel under the title \"A Princess of Mars\" (date approximate).\nOn this day in 1943: Edgar Rice Burroughs` \"The Skeleton Men of Jupiter\" appeared in \"Amazing Stories\" (date approximate). Later included in the anthology \"John Carter of Mars.\"\nOn this day in 1956: Robert A. Heinlein`s \"Double Star\" began serialization in \"Astounding Science Fiction\" (date approximate).\nOn this day in 1977: John Varley`s \"In the Hall of the Martian Kings\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).\nOn this day in 1999: Approximate time setting for \"February 1999: Ylla\" in Ray Bradbury`s \"The Martian Chronicles.\"\nOn this day in 2002: Approximate time setting for \"February 2002: The Locusts\" in Ray Bradbury`s \"The Martian Chronicles.\"\nOn this day in 2003: Approximate time setting for \"February 2003: Interim\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				33 => "On this day in 1896: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the first of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				35 => "On this day in 1694: Christiaan Huygens drew his last sketch of Mars.",
				39 => "On this day in 1972: Mariner 9 frame 4205-78 imaged the Pyramids of Elysium.\nOn this day in 2000: A meteorite, designated GRV 99027, was discovered in the Grove Mountains, Antarctica. The meteorite was later identified as having originated on Mars.\nOn this day in 2001: A meteorite, designated Sayh al Uhaymir 094, was discovered near Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.",
				41 => "On this day in 2010: US President Barack Obama canceled the Constellation program of human expeditions to the Moon and Mars.\nOn this day in 1974: Mars 4 failed to enter Mars orbit, passed Mars.",
				43 => "On this day in 1974: Mars 5 entered Mars orbit.\nOn this day in 1999: The motion picture \"My Favorite Martian\" was released.",
				45 => "On this day in 1963: The motion picture \"The Day Mars Invaded Earth\" was released.",
				46 => "On this day in 1858: William Henry Pickering was born; with Andrew Ellicott Douglass, he devised the first known Martian calendar to be used in scientific research.\nOn this day in 1936: ERB Inc released \"The Swords of Mars.\"\nOn this day in 1940: ERB Inc released \"The Synthetic Men of Mars.\"",
				48 => "Quirinalia, sacred to Quirinus, celebrating the ascension of Romulus. Celebrated by the flamen Quirinalis. It is also known as the Feast of Fools.",
				49 => "On this day in 1910: The first motion picture about Mars, \"A Trip to Mars,\" was released.\nOn this day in 1922: Edgar Rice Burroughs` \"The Chessmen of Mars\" began serialization in \"Argosy All-Story Weekly.\"",
				51 => "On this day in 1898: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 20th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				55 => "On this day in 1897: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the eighth of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				56 => "On this day in 1969: Mariner 6 was launched.",
				57 => "On this day in 1842: Camille Flammarion was born, developed detailed maps of Mars and believed in an advanced civilization on that planet.",
				58 => "Equirria, a festival of horse racing dedicated to Mars. Established by Romulus himself in the early days of Rome. It is held in the Campus Martius in Roma, or the Campus Martialis if the former is flooded. ",
				59 => "On this day in 1928: Edgar Rice Burroughs began dictating \"A Fighting Man of Mars\" on Ediphone.",
				60 => "Feriae Marti, the original New Year`s Day on the Roman calendar. The Salii, a collegium divided into the Palatini (devoted to Mars Gradivus) and the Agonenses (devoted to Quirinus), dressed in archaic military armor and bearing the sacred shields, dance throughout the streets and chant the ancient Carme Saliare. After their dance, the Salii hold a spectacular feast.\nOn this day in 1941: Edgar Rice Burroughs` \"The City of Mummies\" appeared in \"Amazing Stories\" (date approximate). The story was later included in the anthology \"Llana of Gathol.\"\nOn this day in 1953: Lester Del Rey and Erik Van Lhin`s \"Police Your Planet\" began serialization in \"Science Fiction Adventures\" (date approximate).\nOn this day in 1973: Lin Carter`s \"The Man Who Loved Mars\" was published (date approximate).\nOn this day in 2000: Approximate time setting for \"March 2000: The Taxpayer\" in Ray Bradbury`s \"The Martian Chronicles.\nOn this day in 2001: Thomas W. Cronin`s \"As it Is on Mars\" was published.\nOn this day in 2003: Thomas W. Cronin`s \"Give Us This Mars\" was published.\nOn this day in 2005: Thomas W. Cronin`s \"Glory Be to Mars\" was published (date approximate).\nOn this day in 2012: The motion picture \"We Are One\" was released.",
				67 => "On this day in 1899: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 30th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				69 => "On this day in 1974: Mars 7 passed Mars. Lander missed Mars.\nOn this day in 2006: Mars Reconnaissance Orbiter entered Mars orbit.",
				70 => "On this day in 1898: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 21st of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this day in 2012: The motion picture \"John Carter\" was released.",
				71 => "On this day in 1974: Mars 6 passed Mars. Contact with the lander was lost before landing.\nOn this day in 2000: The motion picture \"Mission to Mars\" was released.",
				72 => "On this day in 1855: Percival Lowell was born, developed detailed maps of Mars and believed in an advanced Martian civilization.\nOn this day in 1992: US President George H. W. Bush issued National Space Policy Directive 6, \"Space Exploration Initiative,\" calling for human expeditions to Mars.",
				73 => "Equirria, a festival of horse racing dedicated to Mars. Established by Romulus himself in the early days of Rome. It is held in the Campus Martius in Roma, or the Campus Martialis if the former is flooded.\nOn this day in 1834: Giovanni Schiaparelli was born, developed first detailed maps of Mars and originated the system of areographic nomenclature in use today.\nOn this day in 1897: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the ninth of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				76 => "Agonalia, a festival sacred to Mars.",
				78 => "First day of Quinquatria, a celebration sacred to Mars, the first of five days. The Salii (priests of Mars) dance in the comitium (attended by the pontiffs and the symbolic representatives of the army--the tribuni celerum), and the sacred arma ancilia are purified. In this sense, it is a ritual preparation for the season`s coming military campaigns.",
				79 => "Second day of Quinquatria, a celebration sacred to Mars, the second of five days. The Salii (priests of Mars) dance in the comitium (attended by the pontiffs and the symbolic representatives of the army--the tribuni celerum), and the sacred arma ancilia are purified. In this sense, it is a ritual preparation for the season`s coming military campaigns.",
				80 => "Quinquatria, a celebration sacred to Mars, the third of five days. The Salii (priests of Mars) dance in the comitium (attended by the pontiffs and the symbolic representatives of the army--the tribuni celerum), and the sacred arma ancilia are purified. In this sense, it is a ritual preparation for the season`s coming military campaigns.",
				81 => "Third day of Quinquatria, a celebration sacred to Mars, the fourth of five days. The Salii (priests of Mars) dance in the comitium (attended by the pontiffs and the symbolic representatives of the army--the tribuni celerum), and the sacred arma ancilia are purified. In this sense, it is a ritual preparation for the season`s coming military campaigns.\nOn this day in 2010: Contact with Mars Exploration Rover A (Spirit) was lost.",
				82 => "Fourth day of Quinquatria, a celebration sacred to Mars, the last of five days. The Salii (priests of Mars) dance in the comitium (attended by the pontiffs and the symbolic representatives of the army--the tribuni celerum), and the sacred arma ancilia are purified. In this sense, it is a ritual preparation for the season`s coming military campaigns.\nTubilustrum, the lustration of trumpets, sacred to Mars. An ewe is sacrificed to sanctify the trumpets used in many of the public rites in preparation for both the coming sacral year and the military campaigning season. It is accompanied by a dance of the Salii.\nOn this day in 1912: Wernher von Braun was born, developed the Saturn V launch vehicle, author of \"The Mars Project.\"",
				83 => "On this day in 1899: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 31st of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				84 => "On this day in 1969: Mariner 7 was launched.",
				86 => "On this day in 1969: Mars 1969A failed during launch.\nOn this day in 1989: Contact with Phobos 2 was lost after 58 sols in Mars orbit.",
				87 => "On this day in 1938: Edgar Rice Burroughs began writing \"The Synthetic Men of Mars.\"",
				89 => "On this day in 1899: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 32nd of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				91 => "On this day in 1897: H. G. Wells` \"The War of the Worlds\" began serialization in England in \"Pearson`s\" (date approximate).\nOn this day in 1899: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 33rd of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this day in 1930: Edgar Rice Burroughs` \"A Fighting Man of Mars\" began serialization in \"Blue Book\" (date approximate).\nOn this day in 1952: Leigh Brackett`s \"The Last Days of Shandakor\" appeared in \"Startling Stories\" (date approximate).\nOn this day in 1964: Philip K. Dick`s \"Martian Time-Slip\" was published (date approximate).\nOn this day in 1966: Philip K. Dick`s \"We Can Remember It for You Wholesale\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate). It was the basis of of the 1990 motion picture \"Total Recall.\"\nOn this day in 2003: Approximate time setting for \"April 2003: The Musicians\" in Ray Bradbury`s \"The Martian Chronicles.\"\nOn this day in 2005: Approximate time setting for \"April 2005: Usher II\" in Ray Bradbury`s \"The Martian Chronicles.\"\nOn this day in 2026: Approximate time setting for \"April 2026: The Long Years\" in Ray Bradbury`s \"The Martian Chronicles.\"\nOn this day in 2000: Approximate time setting for \"April 2000: The Third Expedition\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				96 => "On this day in 1952: Ray Bradbury`s \"The Wilderness\" was published in \"The Philadelphia Enquirer.\"\nOn this day in 1953: The motion picture \"Abbott and Costello Go to Mars\" was released.",
				98 => "On this day in 1916: Edgar Rice Burroughs` \"Thuvia, Maid of Mars\" began serialization in \"All-Story Weekly.\"",
				101 => "On this day in 1921: The motion picture \"A Message from Mars\" was released.",
				102 => "On this day in 1980: Contact with Viking Lander 2 was lost after 1,280 sols in Utopia Planitia.\nOn this day in 2002: The motion picture \"Stranded\" was released.",
				104 => "On this day in 1629: Christiaan Huygens was born, drew the first identifiable Martian surface feature.\nOn this day in 1969: Mars 1969B failed during launch.",
				105 => "On this day in 1886: Henri Perrotin and Louis Thollon confirmed the existence of the \"canali.\"",
				109 => "On this day in 2010: The First International Conference on Mars Sedimentology and Stratigraphy opened in El Paso, Texas.",
				110 => "On this day in 1848: Kurd Lasswitz was born, author of \"Two Planets.\"\nOn this day in 1955: The motion picture \"Conquest of Space\" was released.\nOn this day in 1990: The motion picture \"Martians Go Home\" was released.",
				112 => "On this day in 1953: The motion picture \"Invaders From Mars\" was released.\nOn this day in 2014: The Second Humans to Mars Summit opened in Washington, DC.",
				115 => "On this day in 1997: \"The Workshop on Early Mars\" opened in Houston, Texas.",
				117 => "On this day in 1955: The motion picture \"Devil Girl From Mars\" was released.",
				119 => "On this day in 1981: \"The Case For Mars I\" conference opened in Boulder, Colorado.",
				121 => "On this day in 1897: H. G. Wells` \"The War of the Worlds\" began serialization in the USA in \"Cosmopolitan\" (date approximate).\nOn this day in 1950: Ray Bradbury`s \"The Martian Chronicles\" was published (date approximate).\nOn this day in 1963: The television series \"My Favorite Martian\" ended with the 107th episode.\nOn this day in 1979: Robert F. Young`s \"The First Mars Mission\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).\nOn this day in 1993: NASA issued \"Mars Design Reference Mission 1.0\" (date approximate).\nOn this day in 1995: Kim Stanley Robinson`s \"Green Mars\" was published by Spectra.\nOn this day in 1998: A meteorite, designated Dar al Gani 476, was discovered near Dar al Gani, Libya. The meteorite was later identified as having originated on Mars.",
				123 => "On this day in 1990: Voicing his opposition to the Bush Administration`s Space Exploration Initiative, Senator Albert Gore, Jr. told his fellow legislators, \"Before discussing a mission to Mars, the Administration needs a mission to reality.\"\nOn this day in 1991: \"America at the Threshold: America`s Space Exploration Initiative\" (\"The Stafford Report\") called for manned missions to Mars beginning in 2014.",
				125 => "On this day in 2015: The Third Humans to Mars Summit opened in Washington, DC.",
				126 => "On this day in 1915: Orson Welles was born, leader of the Mercury Theater of the Air, which performed the \"Panic Broadcast.\"\nOn this day in 2013: The First Humans to Mars Summit opened in Washington, DC.",
				127 => "On this day in 1856: Angelo Secchi described \"a large, triangular patch.\" Then known as the Hourglass Sea, Secchi instead called it the \"Canale Atlantico,\" the first use of the word \"canale\" applied to Mars. The feature was later named Syrtis Major by Giovanni Schiaparelli.\nOn this day in 1998: A meteorite, designated Dar al Gani 876, was discovered near Dar al Gani, Libya. The meteorite was later identified as having originated on Mars.",
				128 => "On this day in 1971: Mariner 8 failed during launch.",
				130 => "On this day in 1971: Kosmos 419 was launched, but failed to leave Earth orbit.",
				131 => "On this day in 1990: George H. W. Bush became the first US President to set a target date for sending humans to Mars: \"I believe that before America celebrates the 50th anniversary of its landing on the Moon, the American flag should be planted on Mars.\"",
				132 => "On this day in 2001: The First Australian Mars Exploration Conference opened in Melbourne, Victoria.",
				133 => "On this day in 1907: The Martians launched the first of ten cylinders toward Earth, beginning the War of the Worlds.\nOn this day in 2005: \"Star Trek: Enterprise\" episode \"Terra Prime\" aired.",
				134 => "On this day in 1907: During the War of the Worlds, the Martians launched the second of ten cylinders toward Earth.",
				135 => "On this day in 1907: During the War of the Worlds, the Martians launched the third of ten cylinders toward Earth.",
				136 => "On this day in 1907: During the War of the Worlds, the Martians launched the fourth of ten cylinders toward Earth.\nOn this day in 1992: Sailor Mars first appeared in \"An Uncharmed Life\" episode of the animated television series \"Sailor Moon\" (Japan air date).",
				137 => "On this day in 1907: During the War of the Worlds, the Martians launched the fifth of ten cylinders toward Earth.",
				138 => "On this day in 1907: During the War of the Worlds, the Martians launched the sixth of ten cylinders toward Earth.\nOn this day in 1988: Soviet President Mikhail Gorbachev called for a joint unmanned Mars mission with the USA.",
				139 => "On this day in 1907: During the War of the Worlds, the Martians launched the seventh of ten cylinders toward Earth.\nOn this day in 1971: Mars 2 was launched.",
				140 => "On this day in 1907: During the War of the Worlds, the Martians launched the eighth of ten cylinders toward Earth.",
				141 => "On this day in 1907: During the War of the Worlds, the Martians launched the nintn of ten cylinders toward Earth.\nOn this day in 2012: The Third International Conference on Early Mars opened in Lake Tahoe, Nevada.",
				142 => "Tubilustrum, the lustration of trumpets, sacred to Mars. An ewe is sacrificed to sanctify the trumpets used in many of the public rites. It is accompanied by a dance of the Salii.\nOn this day in 1907: During the War of the Worlds, the Martians launched the last of ten cylinders toward Earth.",
				143 => "On this day in 1897: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 10th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this day in 1909: Nikola Tesla discussed communication with Mars in an article in \"The New York Times.\"",
				145 => "On this day in 2008: Phoenix landed in Green Valley, Vastitas Borealis.",
				146 => "On this day in 1988: The Planetary Society`s \"Mars Declaration\" was published in \"The Washington Post.\"\nOn this day in 1993: \"The Case For Mars V\" conference opened in Boulder, Colorado.",
				148 => "On this day in 1971: Mars 3 was launched.",
				149 => "Ambarvalia, the ritual purification of the fields, connected with the agricultural deities Ceres, Bacchus, and Mars. It is the \"beating of the bounds\", when the boundaries between fields are purified by a procession of sacrificial animals, the suovetaurilia.",
				150 => "On this day in 1971: Mariner 9 was launched.",
				151 => "On this day in 1931: Edgar Rice Burroughs` \"A Fighting Man of Mars\" was published in \"Metropolitan.\"",
				152 => "This day is sacred to Mars. It is the anniversary of the dedication of the Temple of Mars near the Capena gate.\nOn this day in 1894: The Lowell Observatory began its study of Mars. Eighteen months later, Percival Lowell was published the observations of the 1894 opposition in a popular book, \"Mars.\"\nOn this day in 1941: Edgar Rice Burroughs` \"The Black Pirates of Barsoom\" appeared in \"Amazing Stories\" (date approximate). The story was later included in the anthology \"Llana of Gathol.\"\nOn this day in 1984: Lin Carter`s \"Down to a Sunless Sea\" was published by DAW (date approximate).\nOn this day in 1990: The motion picture \"Total Recall\" was released.\nOn this day in 2001: Approximate time setting for \"June 2001: And the Moon Be Still as Bright\" in Ray Bradbury`s \"The Martian Chronicles.\"\nOn this day in 2003: Approximate time setting for \"June 2003: Way in the Middle of the Air\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				153 => "On this day in 1895: \"The San Francisco Chronicle\" reported that an observer had read the name of the Almighty in Hebrew letters on the surface of Mars.\nOn this day in 1950: The motion picture \"Rocketship X-M\" was released.\nOn this day in 1997: Kim Stanley Robinson`s \"Blue Mars\" was published by Spectra.\nOn this day in 2003: Mars Express was launched.",
				154 => "This day is sacred to Bellona.\nOn this day in 1963: The first \"Symposium on the Exploration of Mars\" opened in Denver, Colorado. This was the first conference devoted to the exploration of Mars via spacecraft.",
				155 => "On this day in 1899: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the last of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this day in 1990: \"The Case For Mars IV\" conference opened in Boulder, Colorado.\nOn this day in 2005: The motion picture \"Crimson Force\" was released.",
				157 => "On this day in 1986: The motion picture \"Invaders From Mars\" was released.",
				160 => "On this day in 1930: Lin Carter was born, author of \"The Man Who Loved Mars.\"",
				161 => "On this day in 2003: Mars Exploration Rover Spirit was launched.",
				164 => "Minor Quinquatria, a festival sacred to Minerva and Mars.",
				165 => "On this day in 1907: The first Martian cylinder landed on Horsell Common, east of Horsell. The Deputation advanced upon the pit, waving a white flag. There was a flash of light, an invisible ray of heat flashed from man to man, and each burst into flame, killing about 40 people.",
				166 => "On this day in 1907: During the War of the Worlds, the second Martian cylinder landed on the Addlestone Golf Links. A battalion of the Cardigan Regiment rushed the Horsell pit in skirmish order and was annihilated by the Heat-Ray. The fighting-machine then destroyed Woking.",
				167 => "On this day in 1907: During the War of the Worlds, the third Martian cylinder landed north of Pyrford, completing the Surrey Triangle. Five Martian fighting-machines advanced down the Wey River to the confluence of the Thames. Royal Army batteries engage the Martians, destroying one fighting-machine, but Weybridge and Chertsey were destroyed by Heat-Ray. Later, the St. George's Hill battery damaged one fighting-machine, but was then destroyed. Seven Martian fighting-machines fanned out along a curved line between St. George's Hill, Weybridge, and the village of Send. The Martians discharged Black Smoke across the valley of the Thames, advancing through Street Cobham and Ditton. Richmond, Kingston, and Wimbledon were destroyed.",
				168 => "On this day in 1907: During the War of the Worlds, the fourth Martian cylinder landed in Bushey Park, beginning the West End Triangle. The Martians advanced in a line from Hanwell in the north to Coombe and Malden in the south. Organized resistance by the British forces collapsed. The Martians went to and fro over the North Downs between Guildford and Maidstone, using the Black Smoke to eliminate any artillery batteries located there. Police organization in London broke down. The railway system collapsed.\nOn this day in 2000: A meteorite, designated Dhofar 378, was discovered near Dhofar, Oman. The meteorite was later identified as having originated on Mars.",
				169 => "On this day in 1907: During the War of the Worlds, the fifth Martian cylinder landed in Sheen, and the sixth Martian cylinder landed in Wimbledon, completing the West End Triangle.",
				170 => "On this day in 1907: During the War of the Worlds, the seventh Martian cylinder landed in Primrose Hill, where the invaders established their new headquarters. \"HMS Thunder Child\" made a suicide run at three fighting-machines the mouth of the Blackwater to cover the escape of passenger vessels. Two fighting-machines were destroyed. \"Thunder Child\" was also destroyed. \nOn this day in 1963: Mars 1 passed Mars three months after contact was lost.\nOn this day in 1976: Viking 1 entered Mars orbit.\nOn this day in 1995: The \"Mars Together\" conference opened in Palo Alto, California.",
				171 => "On this day in 1907: During the War of the Worlds, the eighth Martian cylinder landed (unreported). A fighting-machine destroyed Leatherhead, with every soul in it.",
				172 => "On this day in 1907: During the War of the Worlds, the ninth Martian cylinder landed (unreported). The Martians vacated the Sheen cylinder except for one fighting-machine and one handling-machine.",
				173 => "On this day in 1907: During the War of the Worlds, the last of ten Martian cylinder landed (unreported).",
				176 => "On this day in 1897: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 11th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				178 => "On this day in 1897: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 12th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this day in 1941: Edgar Rice Burroughs` \"The Black Pirates of Barsoom\" was published in \"Amazing Stories.\"\nOn this day in 2001: A meteorite, designated Sayh al Uhaymir 060, was discovered near Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.",
				179 => "On this day in 1911: An unfortunate dog in Nakhla, Egypt was struck by part of a meteorite and killed. In the 1980s, the Nakhla meteorite was identified as having originated on Mars.",
				180 => "On this day in 2005: The motion picture \"The War of the Worlds\" was released.",
				182 => "On this day in 1911: Writing down his daydreams on the backs of old letterheads of previously failed businesses, Edgar Rice Burroughs used free time at his office to begin \"A Princess of Mars\" (date approximate).\nOn this day in 1960: Arthur C. Clarke`s \"Crime on Mars,\" later titled \"The Trouble with Time,\" was published in \"Ellery Queen\" (date approximate).\nOn this day in 1976: Lin Carter`s \"The Martian El Dorado of Parker Wintley\" was published in \"The DAW Science Fiction Reader\" (date approximate).\nOn this day in 1992: Michael Cassutt`s \"The Last Mars Trip\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).\nOn this day in 1997: William K. Hartmann`s \"Mars Underground\" was published (date approximate).\nOn this day in 1997: NASA issued \"Mars Design Reference Mission 3.0\" (date approximate).\nOn this day in 2009: NASA issued \"Human Exploration of Mars Design Reference Architecture 5.0\" (date approximate).",
				185 => "On this day in 1907: The last Martians succumbed to terrestrial disease, ending the War of the Worlds.\nOn this day in 1997: Mars Pathfinder landed in Ares Vallis. The spacecraft was later renamed the Carl Sagan Memorial Station.\nOn this day in 2008: The Eighth Australian Mars Exploration Conference opened in Adelaide, South Australia.",
				186 => "On this day in 1867: Andrew Ellicott Douglass was born; with William Henry Pickering, he devised the first known Martian calendar to be used in scientific research.",
				187 => "On this day in 1997: Sojourner, the first roving vehicle on Mars, deployed from the Carl Sagan Memorial Station.",
				188 => "On this day in 1907: Robert A. Heinlein was born, author of \"Red Planet,\" \"Podkayne of Mars,\" and \"Stranger in a Strange Land.\"\nOn this day in 1988: Phobos 1 was launched.",
				189 => "On this day in 2003: Mars Exploration Rover Opportunity was launched.",
				190 => "On this day in 2007: The Seventh International Conference on Mars opened in Pasadena, California.\nOn this day in 2010: The Tenth Australian Mars Exploration Conference opened in Melbourne, Victoria.",
				191 => "On this day in 1984: \"The Case For Mars II\" conference opened in Boulder, Colorado.",
				193 => "On this day in 1988: Phobos 2 was launched.\nOn this day in 2002: The Second Australian Mars Exploration Conference opened in Sydney, New South Wales.",
				194 => "On this day in 2007: The Seventh Australian Mars Exploration Conference opened in Perth, Western Australia.",
				195 => "On this day in 2014: The Eighth International Conference on Mars opened in Pasadena, California.",
				196 => "On this day in 1965: Mariner 4 achieved the first flyby of Mars, transmitted 22 images of a cratered surface.",
				198 => "On this day in 1964: The motion picture \"Robinson Crusoe on Mars\" was released.\nOn this day in 1996: \"The Case for Mars VI\" conference opened in Boulder, Colorado.\nOn this day in 2009: The Ninth Australian Mars Exploration Conference opened in Adelaide, South Australia.",
				199 => "On this day in 1958: A few months before it was transformed into a space agency by the National Aeronautics and Space Act, a report by the National Advisory Committee on Aeronautics projected that a manned Mars mission might be undertaken in 1977.\nOn this day in 1987: \"The Case For Mars III\" conference opened in Boulder, Colorado.\nOn this day in 1999: The Fifth International Conference on Mars opened in Pasadena, California.\nOn this day in 2011: A meteorite, designated Tissint, was observed to fall in Tata Province in the Guelmim-Es Semara region of Morocco. The meteorite was later identified as having originated on Mars.",
				201 => "On this day in 1976: Viking Lander 1 achieved first successful landing on Mars in Chryse Planitia. The spacecraft was later renamed the Thomas A. Mutch Memorial Station.\nOn this day in 1989: US President H. W. George Bush launched the Space Exploration Initiative from the steps of the National Air & Space Museum on the 20th anniversary of the Apollo 11 moon landing, calling for \"a journey into tomorrow, a journey to another planet - a manned mission to Mars.\"\nOn this day in 2003: The Sixth International Conference on Mars opened in Pasadena, California.",
				202 => "On this day in 1973: Mars 4 was launched.\nOn this day in 1986: The \"NASA Mars Conference\" opened in Washington, DC.",
				203 => "On this day in 2011: The Eleventh Australian Mars Exploration Conference opened in Perth, Western Australia.",
				204 => "On this day in 1914: Edgar Rice Burroughs began writing \"The Gods of Mars.\"",
				205 => "On this day in 1940: Edgar Rice Burroughs began writing \"John Carter and the Pits of Horz,\" first of a series for new book. The story was later was published under the title \"The City of Mummies.\"\nOn this day in 1948: Marvin the Martian debuted in animated short film \"Haredevil Hare.\"",
				206 => "On this day in 1973: Mars 5 was launched.\nOn this day in 1976: Viking Orbiter 1 frame 35A72 imaged Cydonia Face and Pyramids.\nOn this day in 1978: Contact with Viking Orbiter 2 was lost after 697 sols in Mars orbit.",
				208 => "On this day in 1781: William Herschel calculated a rotational period for Mars of 24 hours, 39 minutes, and 21.67 seconds. He also confirmed that the north polar spot was eccentric to the pole.",
				209 => "On this day in 1894: Nice Observatory astronomer Stephane Javelle observed a luminous projection in the region of the southern terminator of Mars.",
				211 => "On this day in 1894: Henri Joseph Anastase Perrotin, director of the Nice Observatory, sent the following telegram: \"Projection lumineuse dans région australe du terminateur de Mars observeé par Javelle 28 Juillet 16 heures Perrotin.\"\nOn this day in 2009: \"The Twelfth International Mars Society Convention\" opened in College Park, Maryland.",
				212 => "On this day in 1969: Mariner 6 passed Mars, returned 26 near encounter images.\nOn this day in 2003: The Fourth Australian Mars Exploration Conference opened in Adelaide, South Australia.",
				213 => "On this day in 1941: Edgar Rice Burroughs` \"The Yellow Men of Mars\" appeared in :Amazing Stories: (date approximate). The story was later included in the anthology \"Llana of Gathol.\"\nOn this day in 1963: Philip K. Dick`s \"All We Marsmen\" began serialization in \"Worlds of Tomorrow\" (date approximate).\nOn this day in 1968: NASA terminated the contract to procure additional Saturn Vs, ending production with Saturn 515, and abandoning the heavy-lift launch capability required to launch piloted Mars missions.\nOn this day in 1999: Approximate time setting for \"August 1999: The Summer Night\" in Ray Bradbury`s \"The Martian Chronicles.\"\nOn this day in 1999: Approximate time setting for \"August 1999: The Earth Men\" in Ray Bradbury`s \"The Martian Chronicles.\"\nOn this day in 2000: A meteorite, designated Sayh al Uhaymir 051, was discovered near Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.\nOn this day in 2001: Approximate time setting for \"August 2001: The Settlers\" in Ray Bradbury`s \"The Martian Chronicles.\"\nOn this day in 2002: Approximate time setting for \"August 2002: Night Meeting\" in Ray Bradbury`s \"The Martian Chronicles.\"\nOn this day in 2005: Approximate time setting for \"August 2005: The Old Ones\" in Ray Bradbury`s \"The Martian Chronicles.\"\nOn this day in 2026: Approximate time setting for \"August 2026: There Will Come Soft Rains\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				214 => "On this day in 1894: The British scientific journal \"Nature\" reported the observation of Stephane Javelle.\"If we assume the light to be on the planet itself, then it must either have a physical or human origin; so it is expected that the old idea that the Martians are signalling to us will be revived.\"H. G. Wells' narrator later speculated, \"I am inclined to think that this blaze may have been the casting of the huge gun, in the vast pit sunk into their planet, from which their shots were fired at us.\"",
				215 => "On this day in 1945: The motion picture \"The Purple Monster Strikes\" was released.\nOn this day in 1967: NASA Manned Spacecraft Center issued \"Request for Proposal No. BG721-28-7-528P, Planetary Surface Sample Return Probe Study for Manned Mars/Venus Reconnaissance/Retrieval Missions\" in the 1975-1982 time frame. Rep. Joseph Karth, acting chairman of the House Subcommittee on NASA Oversight, who had been fighting an uphill battle to preserve Project Voyager funding, later expressed his exasperation, for the move cast the program of Saturn V-launched unmanned orbiters and landers in the role of a foot in the door for manned follow-on missions to the planets: \"Very bluntly, a manned mission to Mars or Venus by 1975 or 1977 is now and always has been out of the question, and anyone who persists in this kind of misallocation of resources at this time will be stopped.\"\nOn this day in 2006: \"The Ninth International Mars Society Convention\" opened in Washington, DC.\nOn this day in 2012: \"The Fifteenth International Mars Society Convention\" opened in Pasadena California.",
				216 => "On this day in 1976: Viking Orbiter 1 frames 43A01 through 43A04 imaged Deuteronilus Crater Pyramid.\nOn this day in 2007: Phoenix lander was launched.\nOn this day in 2011: \"The Fourteenth International Mars Society Convention\" opened in Dallas, Texas.",
				217 => "On this day in 1969: Mariner 7 passed Mars, returned 33 near encounter images.\nOn this day in 1973: Mars 6 was launched.\n\"On this day in 2010: The Thirteenth International Mars Society Convention\" opened in Dayton, Ohio.",
				218 => "On this day in 1965: Zond 2 passed Mars four months after contact was lost.\nOn this day in 1996: The National Science Foundation announced evidence for possible early life on Mars in Antarctic meteorite ALH84001.\nOn this day in 2001: \"The Infinite Worlds of H.G. Wells\" episode \"The Crystal Egg\" aired.\nOn this day in 2012: Mars Science Laboratory \"Curiosity\" landed in Aeolis Palus (\"Bradbury Landing\") in Gale Crater.",
				219 => "On this day in 1976: Viking 2 entered Mars orbit.\nOn this day in 1980: Contact with Viking Orbiter 1 was lost after 1,469 sols in Mars orbit.\nOn this day in 2014: \"The Seventeenth International Mars Society Convention\" opened in League City, Texas.",
				220 => "On this day in 2002: \"The Fifth International Mars Society Convention\" opened in Boulder, Colorado.",
				221 => "On this day in 1965: \"I say let`s do it quickly and establish a foothold on a new planet while we still have one left to take off from.\" --Wernher von Braun, \"Manned Mars Landing,\" \"Aviation Week & Space Technology.\"\nOn this day in 1973: Mars 7 was launched.",
				222 => "On this day in 1877: Asaph Hall gave up a search for Martian moons. The following night, having resumed his search at the insistence of his wife, Angelina, he detected a faint object near Mars, which he later named Deimos.\nOn this day in 2000: \"The Third International Mars Society Convention\" opened in Toronto, Ontario.",
				223 => "On this day in 1989: \"Mars is essentially in the same orbit. Mars is somewhat the same distance from the sun, which is very important. We have seen pictures where there are canals, we believe, and water. If there is water, that means there is oxygen. If oxygen, that means we can breathe.\" -- US Vice President Dan Quayle.\n\"On this day in 2005: The Eighth International Mars Society Convention\" opened in Boulder, Colorado.",
				224 => "On this day in 1877: \"The New York Times\" asked, \"Is Mars inhabited?\" in an editorial. As the best opposition since 1798 approached, questions in the popular mind came to the fore and the possibility of life on Mars was discussed in the press.\nOn this day in 1999: \"The Second International Mars Society Convention\" opened in Boulder, Colorado.\nOn this day in 2005: Mars Reconnaissance Orbiter was launched.",
				225 => "On this day in 1998: \"The Founding Convention of the Mars Society\" opened in Boulder, Colorado.",
				226 => "On this day in 2003: \"The Sixth International Mars Society Convention\" opened in Eugene, Oregon.\n\"On this day in 2008: The Eleventh International Mars Society Convention\" opened in Boulder, Colorado.",
				227 => "On this day in 1998: \"The Founding Declaration of the Mars Society\" was adopted unanimously in Boulder, Colorado.\nOn this day in 2013: \"The Sixteenth International Mars Society Convention\" opened in Boulder, Colorado.",
				228 => "On this day in 1967: U.S. Congress deleted funding for Voyager, a program of Saturn V-launched unmanned orbiters and landers, then scheduled for its first missions in 1973. The program was later down-scoped and renamed \"Viking.\"",
				229 => "On this day in 1877: Asaph Hall discovered Phobos.",
				230 => "On this day in 1845: Omsby MacKnight Mitchel observed a white patch detached from the south polar cap of Mars. The feature became known as the Mountains of Mitchel. Actually it is a depression.\nOn this day in 1877: Asaph Hall announced the discovery of Mars` two moons. At the suggestion of Henry Madan, the Science Master of Eton, England, Hall named the moons Phobos and Deimos.",
				231 => "On this day in 2005: The Fifth Australian Mars Exploration Conference opened in Canberra, ACT.",
				232 => "On this day in 1956: A dust storm began with a bright cloud over the Hellas-Noachis region that spread to engulf the whole planet by mid-September.\nOn this day in 1975: Viking 1 was launched.\nOn this day in 2004: \"The Seventh International Mars Society Convention\" opened in Chicago, Illinois.",
				233 => "On this day in 1898: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 22nd of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this day in 1907: I. M. Levitt was born, inventor of a Martian calendar and the first Earth-Mars mechanical date-time computer.\nOn this day in 1999: A meteorite, designated Dar al Gani 975, was discovered near Dar al Gani, Libya. The meteorite was later identified as having originated on Mars.\nOn this day in 2000: The Second International Conference on Mars Polar Science and Exploration opened in Reykjavik, Iceland.",
				234 => "On this day in 1897: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 13th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this day in 1920: Ray Bradbury was born, author of \"The Martian Chronicles.\"\nOn this day in 1993: Contact with Mars Observer was lost enroute to Mars.\nOn this day in 2003: The Third Australian Mars Exploration Conference opened in Perth, Western Australia.",
				235 => "On this day in 2001: \"The Fourth International Mars Society Convention\" opened at Stanford University, California.",
				236 => "On this day in 1638: Two years after making his first drawing of Mars, Neapolitan lawyer and amateur astronomer Francesco Fontana produced a second drawing, which like the first, featured a dark spot, which has been attributed to an optical defect in his telescope.",
				237 => "On this day in 1865: A meteorite was observed to fall near Shergotty, India. In the 1980s, the Shergotty meteorite was identified as having originated on Mars.\nOn this day in 1993: Mars Observer passed Mars three days after contact was lost.",
				238 => "On this day in 1953: The motion picture \"The War of the Worlds\" was released.\nOn this day in 2003: In perihelic opposition, Mars made its closest approach to Earth in 60,000 years, coming within 55.8 million kilometers.",
				239 => "On this day in 1911: \"Martians Build Two Immense Canals in Two Years\" reported in \"The New York Times.\"",
				240 => "On this day in 1877: Henry Draper of New York and Edward Singleton Holden of the U.S. Naval Observatory in Washington claimed to have jointly discovered a third moon of Mars at Draper`s private observatory at Hastings-on-the-Hudson. This discovery proved to be false; in fact, the proposed moon did not even obey Kepler`s laws.",
				242 => "On this day in 1976: Viking Orbiter 1 frame 70A13 imaged the Cydonia Face and Pyramids.",
				243 => "On this day in 1981: The Thied International Colloquium on Mars opened in Pasadena, California.",
				244 => "On this day in 1875: Edgar Rice Burroughs was born, author of the \"Barsoom\" series.\nOn this day in 1969: A NASA report to US President Richard Nixon`s Space Task Group, which was chartered to explore options for space efforts beyond the Apollo program, stated, \"Manned expeditions to Mars could begin as early as 1981.\"\nOn this day in 1993: Sailor Mars first appeared in the manga \"Run Run.\"\nOn this day in 2005: Approximate time setting for \"September 2005: The Martian\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				245 => "On this day in 1988: Contact with Phobos 1 was lost en route to Mars.\nOn this day in 2007: \"The Tenth International Mars Society Convention\" opened in Los Angeles, California.",
				246 => "On this day in 1976: Viking Lander 2 landed in Utopia Planitia.",
				247 => "On this day in 1898: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 23rd of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				248 => "On this day in 2014: The Fourteenth European Mars Convention opened in Podzamcze, Poland.",
				252 => "On this day in 1975: Viking 2 was launched.",
				254 => "On this day in 2002: Alex Irvine`s \"Pictures from an Expedition\" was published in \"In The Magazine of Fantasy and Science Fiction.\"",
				255 => "On this day in 1877: Giovanni Schiaparelli began a detailed study of Mars for the purpose of drawing a new and accurate map of the planet.\nOn this day in 1897: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 14th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this day in 1997: Mars Global Surveyor entered Mars orbit.\nOn this day in 2011: The Third International Conference on Mars Polar Science and Exploration opened in Fairbanks, Alaska.",
				257 => "On this day in 2012: The Twelfth Australian Mars Exploration Conference opened in Canberra, ACT.",
				258 => "On this day in 1877: Giovanni Schiaparelli observed the canale Cyclops.\nOn this day in 1969: The report by US President Richard Nixon`s Space Task Group, \"The Post-Apollo Space Program: Directions for the Future,\" stated, \"We conclude that NASA has the demonstrated organizational competence and technology base, by virtue of the Apollo success and other achievements, to carry out a successful program to land man on Mars within 15 years.\" Nevertheless, the report backed away from an early manned landing on Mars, recommending that the focus for the next decades in space should be on the development of hardware and systems that would ultimately support a manned mission to Mars at the close of the 20th century.",
				261 => "On this day in 1976: Viking Orbiter 1 frame 86A10 imaged the Utopia Faces.",
				262 => "On this day in 1995: Sailor Mars first appeared in \"An Uncharmed Life\" episode of the animated television series \"Sailor Moon\" (USA air date).\nOn this day in 1996: US President Bill Clinton abandoned George Bush`s goal of a manned landing on Mars. His National Science and Technology Council`s statement of National Space Policy only mentioned \"a sustained program to support a robotic presence on the surface of Mars by year 2000 for the purposes of scientific research, exploration and technology development.\"\nOn this day in 2013: The motion picture \"The Last Days on Mars\" was released.",
				263 => "On this day in 1896: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the second of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				264 => "On this day in 1866: H. G. Wells was born, author of \"The War of the Worlds.\"",
				265 => "On this day in 1877: Giovanni Schiaparelli observed the canale Ambrosia.\nOn this day in 2014: Mars Atmosphere and Volatile EvolutioN Mission (MAVEN) entered Mars orbit.",
				266 => "On this day in 1999: Two meteorites, designated Los Angeles 001 and 002, were discovered in the Mojave Desert near Los Angeles, California. The meteorites were later identified as having originated on Mars.",
				267 => "On this day in 2014: Mars Orbiter Mission (MOM), also called Mangalyaan, entered Mars orbit.",
				268 => "On this day in 1924: The motion picture \"Aelita: Queen of Mars\" was released.\nOn this day in 1992: Mars Observer was launched.",
				269 => "On this day in 2003: The Third European Mars Convention opened in Bremen, Germany.",
				270 => "On this day in 1919: Edgar Rice Burroughs` \"The Warlord of Mars\" was published by McClurg.\nOn this day in 1940: Edgar Rice Burroughs began writing \"The Black Pirates of Barsoom,\" part 2 of a new Mars series.\nOn this day in 2002: The Second European Mars Convention opened in Rotterdam, Netherlands.",
				271 => "On this day in 2001: The First European Mars Convention opened in Paris, France.",
				272 => "On this day in 1963: The television series \"My Favorite Martian\" debuted.",
				273 => "On this day in 2011: The Eleventh European Mars Convention opened in Neuchatel, Switzerland.\nOn this day in 2013: The Thirteeth Australian Mars Exploration Conference opened in Sydney, New South Wales.",
				274 => "On this day in 1783: William Herschel observed the south polar cap of Mars. \"I am inclined to think that the white spot has some little revolution.... It is rather probable that the real pole, though within the spot, may lie near the circumference of it, or one-third of its diameter from one of the sides. A few days more will show it, as I shall now fix my particular attention on it.\"\nOn this day in 1895: \"The observations of 1894 have made it practically certain that the so-called `canals` of Mars are real, whatever may be their explanation,\" Princeton astronomer Charles A. Young declared in his article for \"Cosmopolitan,\" \"Mr. Lowell`s Theory of Mars\" (date approximate).\nOn this day in 1954: Alfred Coppel`s \"Mars Is Ours\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).\nOn this day in 1964: Leigh Brackett`s \"Purple Priestess of the Mad Moon\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).\nOn this day in 1977: Lin Carter`s \"The City Outside the World\" was published by Berkeley (date approximate).\nOn this day in 1993: Kim Stanley Robinson`s \"Red Mars\" was published by Spectra.\nOn this day in 2002: Approximate time setting for \"October 2002: The Shore\" in Ray Bradbury`s \"The Martian Chronicles.\"\nOn this day in 2026: Approximate time setting for \"October 2026: The Million-Year Picnic\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				275 => "On this day in 2006: The Fourth International Conference on Mars Polar Science and Exploration opened in Davos, Switzerland.",
				276 => "On this day in 1815: A meteorite was observed to fall near Chassigny, France. In the 1980s, the Chassigny meteorite was identified as having originated on Mars. It was the first Martian meteorite to be discovered.\nOn this day in 1898: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 24th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this day in 1913: Edgar Rice Burroughs drew a map of Barsoom at the request of his publisher.\nOn this day in 1962: A meteorite was observed to fall near Zagami, Nigeria. In the 1980s, this meteorite was identified as having originated on Mars.\nOn this day in 2000: Kim Stanley Robinson`s \"The Martians\" was published by Spectra.",
				277 => "On this day in 1877: Giovanni Schiaparelli observed the canali Ganges and Phison.",
				280 => "On this day in 1997: Contact with the Carl Sagan Memorial Station (Mars Pathfinder) was lost after 93 sols in Ares Vallis.",
				281 => "On this day in 1976: \"Today we have touched Mars. There is life on Mars and it is us--extensions of our eyes in all directions, extensions of our mind, extensions of our heart and soul have touched Mars today. That`s the message to look for there: We are on Mars. We are the Martians!\" --Ray Bradbury, speaking at \"The Search for Life in the Solar System\" symposium in Pasadena, California.\nOn this day in 2002: A meteorite, designated Sayh al Uhaymir 150, was discovered near Dar al Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.\nOn this day in 2008: The motion picture \"Interplanetary\" was released.",
				282 => "On this day in 2004: The Fourth European Mars Convention opened in Milton Keynes, England.",
				283 => "On this day in 1877: Giovanni Schiaparelli observed Mare Erythraeum and Noachis to be obscured by clouds.\nOn this day in 1897: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 15th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this day in 1917: Edgar Rice Burroughs` \"A Princess of Mars\" was published by McClurg.\nOn this day in 1960: Mars 1960A, the first known Mars mission, failed during launch.",
				284 => "On this day in 1997: The motion picture \"RocketMan\" was released.\nOn this day in 2010: US President Obama signed the NASA Authorization Act of 2010, which authorized a three-year funding commitment for continued development of the Orion spacecraft and the development of a new heavy-lift Space Launch System to support eventual manned lunar and interplanetary missions; however, the pace of these programs was slowed considerably in comparison to the canceled Constellation program.",
				285 => "On this day in 1951: \"Tales of Tomorrow\" episode \"The Crystal Egg\" aired.\nOn this day in 2012: The Twelfth European Mars Convention opened in Munich, Germany.",
				286 => "On this day in 2003: The Third International Conference on Mars Polar Science and Exploration opened in Lake Louise, Alberta.\nOn this day in 2006: The Sixth Australian Mars Exploration Conference opened in Melbourne, Victoria.",
				287 => "On this day in 1898: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 25th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this day in 1960: Mars 1960B failed during launch.",
				288 => "Equus October, a cavalry exercise for the Roman army, a festival sacred to Mars.\nOn this day in 1829: Asaph Hall was born, discoverer of Phobos and Deimos.\nOn this day in 2009: The Ninth European Mars Convention opened in Bergamo, Italy.",
				290 => "On this day in 2008: The Eighth European Mars Convention opened in Antwerp, Belgium.",
				291 => "On this day in 1896: Charles A. Young of Princeton University discussed the question \"Is Mars Inhabited?\" in \"The Boston Herald.\"\nOn this day in 1998: The First International Conference on Mars Polar Science and Exploration opened in Houston, Texas.",
				292 => "Armilustrum, the lustration of shields, a festival sacred to Mars.\nOn this day in 2007: The Seventh European Mars Convention opened in Delft, Netherlands.",
				293 => "On this day in 1877: Giovanni Schiaparelli observed the canale Eunostos.\nOn this day in 2006: The Sixth European Mars Convention opened in Paris, France.",
				295 => "On this day in 1898: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 26th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this day in 2010: The Tenth European Mars Convention opened in Warsaw, Poland.",
				296 => "On this day in 2001: Mars Odyssey entered Mars orbit.",
				297 => "On this day in 1897: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 16th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this day in 1940: Edgar Rice Burroughs began writing \"Escape on Mars,\" part 3 of a new Mars series. The story was later was published under the title \"The Yellow Men of Mars.\"\nOn this day in 1962: Mars 1962A (Sputnik 29) was launched, but failed to leave Earth orbit.",
				298 => "On this day in 1941: Edgar Rice Burroughs began writing \"The Skeleton Men of Jupiter,\" the first of a planned new John Carter series.\nOn this day in 2013: The Thirteenth European Mars Convention opened in Paris, France.",
				300 => "On this day in 1879: Discussing the canali in a letter to Nathaniel Green, Giovanni Schiaparelli declared, \"It is [as] impossible to doubt their existence as that of the Rhine on the surface of the Earth.\"\nOn this day in 1898: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 27th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this day in 1972: Contact with Mariner 9 was lost after 338 sols in Mars orbit.\nOn this day in 2006: The motion picture \"Fascisti su Marte\" was released.",
				301 => "On this day in 2008: Contact with Phoenix lander was lost.",
				303 => "On this day in 1938: \"The Mercury Theater of the Air,\" starring Orson Welles, performed Howard Koch`s radio play \"Invasion from Mars.\" The broadcast terrorized the eastern USA.",
				305 => "On this day in 1934: Edgar Rice Burroughs` \"The Swords of Mars\" began serialization in \"Blue Book\" (date approximate).\nOn this day in 1952: Isaac Asimov`s \"The Martian Way\" appeared in \"Galaxy Science Fiction\" (date approximate).\nOn this day in 1955: Fredric Brown`s \"Martians, Go Home\" was published (date approximate).\nOn this day in 1957: Isaac Asimov`s \"I`m in Marsport without Hilda\" appeared in \"Venture Science Fiction\" (date approximate).\nOn this day in 1962: Mars 1 was launched. Robert A. Heinlein`s \"Podkayne of Mars\" began serialization in \"If\" (date approximate).\nOn this day in 1963: Roger Zelazny`s \"A Rose for Ecclesiastes\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).\nOn this day in 1964: \"The Outer Limits\" episode \"The Invisible Enemy\" aired.On this day in 2005: Approximate time setting for \"November 2005: The Luggage Store\" in Ray Bradbury`s \"The Martian Chronicles.\"\nOn this day in 2005: Approximate time setting for \"November 2005: The Off Season\" in Ray Bradbury`s \"The Martian Chronicles.\"\nOn this day in 2005: Approximate time setting for \"November 2005: The Watchers\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				306 => "On this day in 1898: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 28th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				308 => "On this day in 1962: Mars 1962B (Sputnik 31) was launched, but failed to leave Earth orbit.\nOn this day in 2005: The Fifth European Mars Convention opened in Swindon, England.",
				309 => "On this day in 1964: Mariner 3 failed during launch.\nOn this day in 2013: Mars Orbiter Mission (MOM), also called Mangalyaan, was launched.",
				311 => "On this day in 1938: The motion picture \"Mars Attacks the World\" was released.\nOn this day in 1996: Mars Global Surveyor was launched.",
				313 => "On this day in 1934: Carl Sagan was born, first president of the Planetary Society.",
				314 => "On this day in 1879: Giovanni Schiaparelli discovered a small white patch in Tharsis and named it Nix Olympica. Nearly a century later, Mariner 9 imagery revealed this feature to be the largest mountain in the Solar System, and the feature was renamed Olympus Mons.\nOn this day in 1911: Percival Lowell reported \"Frost on Mars\" to \"The New York Times.\"\nOn this day in 2000: The motion picture \"Red Planet\" was released.",
				315 => "On this day in 1875: Earl C. Slipher was born, observed Mars extensively.\nOn this day in 1951: The motion picture \"Flight to Mars\" was released.\nOn this day in 1964: The motion picture \"Pajama Party\" (also titled \"The Maid and the Martian\") was released.",
				317 => "On this day in 1982: Contact with the Thomas A. Mutch Memorial Station (Viking Lander 1) was lost after 2,244 sols in Chryse Planitia.",
				318 => "On this day in 1971: Mariner 9 became the first spacecraft to orbit Mars.",
				319 => "On this day in 1996: The motion picture \"Space Jam\" starring Marvin the Martian was released.",
				320 => "On this day in 1996: Mars 96 failed during launch.",
				321 => "On this day in 1996: The third annual \"Lunar and Mars Exploration\" conference was held in San Diego, California.\nOn this day in 2002: A meteorite, designated Sayh al Uhaymir 120, was discovered near Dar al Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.",
				322 => "On this day in 1940: Edgar Rice Burroughs began writing \"The Invisible Men of Mars,\" part 4 of a new Mars series.\nOn this day in 2013: Mars Atmosphere and Volatile EvolutioN Mission (MAVEN) was launched.",
				323 => "On this day in 2003: A meteorite, designated Sayh al Uhaymir 125, was discovered near Dar al Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.",
				324 => "On this day in 1989: NASA`s \"Report of the 90-Day Study on Human Exploration of the Moon and Mars\" estimated the cost of a manned Mars program at $450 billion. Political support for manned Mars missions subsequently collapsed.",
				327 => "On this day in 1930: The motion picture \"Just Imagine\" was released.\nOn this day in 1959: The motion picture \"The Angry Red Planet\" was released.",
				330 => "On this day in 1999: Two meteorites, designated Sayh al Uhaymir 005 and 008, were discovered near Sayh al Uhaymir, Oman. The meteorites were later identified as having originated on Mars.\nOn this day in 2011: Mars Science Laboratory \"Curiosity\" was launched.",
				331 => "On this day in 1971: Mariner 9 returned the first image of Deimos. Mars 2 Orbiter entered Mars orbit. Mars 2 Lander impacted on Mars.",
				332 => "On this day in 1659: Christiaan Huygens drew the first sketch of Mars, including a dark, triangular area later named Syrtis Major, the first Martian surface feature identified from Earth. Huygens used his own design of telescope, which was of much higher quality than that of his predecessors and allowed a magnification of 50 times.\nOn this day in 1897: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 17th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this day in 1964: Mariner 4 was launched.\nOn this day in 1971: The First International Colloquium on Mars opened in Pasadena, California.",
				333 => "On this day in 1896: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the third of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this day in 1922: Edgar Rice Burroughs` \"The Chessmen of Mars\" was published by McClurg.\nOn this day in 2000: A meteorite, designated Y000593, was discovered in the Yamato Mountains, Antarctica. The meteorite was later identified as having originated on Mars.",
				334 => "On this day in 1964: Zond 2 was launched.\nOn this day in 1971: Mariner 9 returned the first image of Phobos.",
				335 => "On this day in 1659: Christiaan Huygens noted, \"The rotation of Mars, like that of the Earth, seems to have a period of 24 hours.\"\nOn this day in 1895: Percival Lowell`s \"Mars\" was published (date approximate).\nOn this day in 1913: Edgar Rice Burroughs` \"The Warlord of Mars\" began serialization in \"All-Story\" (date approximate).\nOn this day in 1974: Lin Carter`s \"The Valley Where Time Stood Still\" was published by DAW (date approximate).\nOn this day in 2001: Approximate time setting for \"December 2001: The Green Morning\" in Ray Bradbury`s \"The Martian Chronicles.\"\nOn this day in 2005: Approximate time setting for \"December 2005: The Silent Towns\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				336 => "On this day in 1971: Mars 3 Orbiter entered Mars orbit. Contact with Mars 2 Lander was lost shortly after landing. No useful data was returned.",
				337 => "On this day in 1999: Contact with Mars Polar Lander was lost prior to landing on Mars.\nOn this day in 2000: A meteorite, designated Y000749, was discovered in the Yamato Mountains, Antarctica. The meteorite was later identified as having originated on Mars.",
				338 => "On this day in 1896: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the fourth of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this day in 1996: Mars Pathfinder was launched.\nOn this day in 1998: A meteorite, designated Yamato 980459, was discovered in the Yamato Mountains, Antarctica. The meteorite was later identified as having originated on Mars. The Outer Limits episode \"Phobos Rising\" aired.\nOn this day in 2014: The Orion spacecraft, designed for manned interplanetary missions, was tested successfully in Earth orbit on an unmanned flight.",
				339 => "On this day in 1898: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 29th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\nOn this day in 1961: NASA`s Office of Manned Space Flight published its \"Long Range Plan,\" which projected the launch of a manned Mars flyby mission in 1970 and a manned Mars landing mission in 1975.",
				341 => "On this day in 1915: Leigh Brackett was born, author of the Eric John Stark series of Martian stories.\nOn this day in 1988: Soviet President Mikhail Gorbachev called for a joint manned Mars mission with the USA.",
				345 => "On this day in 1998: Mars Climate Orbiter was launched.",
				347 => "On this day in 1896: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the fifth of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.n\nOn this day in 1996: The motion picture \"Mars Attacks!\" was released.",
				349 => "On this day in 1896: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the sixth of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.n\nOn this day in 2003: A meteorite, designated MIL 03346, was discovered in the Miller Range of the Transantarctic Mountains. The meteorite was later identified as having originated on Mars.",
				350 => "On this day in 1917: Arthur C. Clarke was born, author of \"The Sands of Mars.\"\nOn this day in 1928: Philip K. Dick was born, author of \"Martian Time-Slip.\"\nOn this day in 1994: A meteorite, designated QUE 94201, was discovered in the Queen Alexandra Range, Antarctica. The meteorite was later identified as having originated on Mars.",
				356 => "On this day in 1958: The motion picture \"Quatermass and the Pit\" was released.\nOn this day in 1988: A meteorite, designated LEW 88516, was discovered at Lewis Cliff, Antarctica. The meteorite was later identified as having originated on Mars.",
				358 => "On this day in 1644: Two patches on the lower part of the disk of Mars were described by a Neapolitan Jesuit named Father Bartoli.",
				359 => "On this day in 2003: Mars Express entered orbit. Contact with Beagle 2 was lost prior to landing in Isidis Planitia.",
				360 => "On this day in 1957: The motion picture \"Mars and Beyond\" was released.",
				361 => "On this day in 1984: A meteorite, designated ALH 84001, was discovered in the Allan Hills, Antarctica. In 1993, the meteorite was identified as having originated on Mars. In 1996, a team of scientists announced evidence on fossilized Martian life in the meteorite.",
				363 => "On this day in 1930: The motion picture \"Mars\" was released.\nOn this day in 1977: A meteorite, designated ALHA 77005, was discovered in the Allan Hills, Antarctica. In the 1980s, the meteorite was identified as having originated on Mars.\nOn this day in 2009: The motion picture \"Princess of Mars\" was released.",
				364 => "On this day in 1610: Galileo Galilei discovered that, like the Moon, Mars had a phase.",
				365 => "On this day in 1864: Robert G. Aitken was born, developed the first complete Martian calendar.",
				_ => string.Empty
			};

			eMonthName = eMonth switch
			{
				1 => "January",
				2 => "February",
				3 => "March",
				4 => "April",
				5 => "May",
				6 => "June",
				7 => "July",
				8 => "August",
				9 => "September",
				10 => "October",
				11 => "November",
				12 => "December",
				_ => string.Empty
			};
		}

		static void ConvEarthToMars()
		{
			double daysSince, solsSince;

			// Range checking:
			if (eYear < 0)
				FormatWrite("Warning: Dates before the year 1 are not handled exactly by this applet.\n");
			else if (eYear < 1582)
				FormatWrite("Warning: Gregorian calendar did not exist in the year specified.\n");
			else if (eYear < 1753)
				FormatWrite("Warning: The British Empire did not adopt the Gregorian calendar until 1752.\n");

			if (eMonth < 1 || eMonth > 12)
			{
				FormatWrite("There are not that many months on Earth.\n");
				DateSuccess = false;
				return;
			}

			if (eDay < 1
				|| eDay > eDaysInMonth[eMonth]
				|| (!IsEarthLeapYear(eYear)
					&& eMonth == 2
					&& eDay > 28))
			{
				FormatWrite("There are not that many days in this month on Earth.\n");
				DateSuccess = false;
				return;
			}

			// Convert to straight days:

			daysSince = GetEarthDaysFromForm();
			daysSince += new DateTime().ToLocalTime().DayOfYear / 1440;

			// Convert days to sols:

			solsSince = (daysSince - EPOCH_OFFSET) / MARS_TO_EARTH_DAYS;

			// Convert back to date, and put it it form:
			SetMarsDateFromSols(solsSince);
			SetEarthDateFromDays(daysSince);
		}

		static void ConvMarsToEarth()
		{
			double solsSince, daysSince;

			// Range checking:
			if (mMonth < 1 || mMonth > 24)
			{
				FormatWrite("There are not that many months on Mars.\n");
				DateSuccess = false;
				return;
			}

			if (mDay < 1
				|| mDay > 28
				|| (mMonth % 6 == 0
					&& mDay == 28
					&& !(mMonth == 24 && IsMartianLeapYear(mYear))
				))
			{
				FormatWrite("There are not that many days in this month on Mars.\n");
				DateSuccess = false;
				return;
			}

			// Convert Martian date to sols:

			solsSince = GetMarsSolsFromForm();
			solsSince -= new DateTime().ToLocalTime().DayOfYear / 365;

			// Convert sols to days:

			daysSince = solsSince * MARS_TO_EARTH_DAYS + EPOCH_OFFSET
			+ ROUND_UP_SECOND;

			// Convert back to date, and put it in form:
			SetEarthDateFromDays(daysSince);
			SetMarsDateFromSols(solsSince);
		}

		static bool IsEarthLeapYear(int year)
		{
			if ((year % 400) == 0) return true;
			if ((year % 100) == 0) return false;
			if ((year % 4) == 0) return true;
			return false;
		}

		static int GetMartianSeasonFromSol(int sol)
		{
			if (sol < 167) return 0;
			if (sol < 334) return 1;
			if (sol < 501) return 2;
			return 3;
		}

		static bool IsMartianLeapYear(int year)
		{
			if ((year % 500) == 0) return true;
			if ((year % 100) == 0) return false;
			if ((year % 10) == 0) return true;
			if ((year % 2) == 0) return false;
			return true;
		}

		static void WriteDates()
		{
			if (earthDate)
				ConvEarthToMars();
			else
				ConvMarsToEarth();

			if (DateSuccess == true)
			{
				string msg = eDayName + ", " + eMonthName + " " + eDay + ", " + eYear + " ";
				if (eHour < 10)
					msg += "0";
				msg += eHour + ":";
				if (eMin < 10)
					msg += "0";
				msg += eMin + ":";
				if (eSec < 10)
					msg += "0";
				msg += eSec + "." + (int)(eMil / 100.0) + "\nWeekday " + eDayOfWeek + "/7, Month " + eMonth + "/12\n\n";

				msg += mSolName + ", " + mMonthName + " " + mDay + ", " + mYear + " ";
				if (mHour < 10)
					msg += "0";
				msg += mHour + ":";
				if (mMin < 10)
					msg += "0";
				msg += mMin + ":";
				if (mSec < 10)
					msg += "0";
				msg += mSec + "." + mMil / 100 + "\nWeekday " + mDayOfWeek + "/7, Month " + mMonth + "/24\n\n";
				msg += ThisDay + "\n\n" + ThisSol + "\n\n";

				while (msg.Contains("\n\n\n"))
					msg = msg.Replace("\n\n\n", "\n\n");

				msg = msg.Replace("\n\n", "\n \n");

				string[] cLines = msg.Split('\n');
				var tmpLineCount = cLines.GetLength(0);

				if (tmpLineCount != cLineCount)
				{
					Console.Clear();
					cLineCount = tmpLineCount;
				}

				Console.CursorVisible = false;
				Console.SetCursorPosition(0, 0);
				FormatWrite(msg);
				Console.CursorVisible = true;
			}
			else
			{
				FormatWrite("A fatal error occurred while converting this date.");
				DateSuccess = true;
			}
		}

		static void Main()
		{
			bool _APPRUNNING = true;
			string input;
			InSolNomen = "1";

			FormatWrite("DarianDay: A Gregorian-to-Darian Calendar Converter\nBased on the JavaScript Darian Calculator available at http://ops-alaska.com");

			while (_APPRUNNING == true)
			{
				if (earthDate)
					FormatWrite("\nEnter an Earth date in the format: HH mm SS.S DD MM YYYY\nOr, you may press Enter to convert the current date and time.\nEnter \'n\' to select a sol nomenclature system.\nEnter \'r\' to run the clock.\nEnter \'s\' to convert Mars time to Earth time.");
				else
					FormatWrite("\nEnter a Mars date in the format: HH mm SS.S DD MM YYYY\nOr, you may press Enter to convert the current date and time.\nEnter \'n\' to select a sol nomenclature system.\nEnter \'r\' to run the clock.\nEnter \'s\' to convert Earth time to Mars time.");

				Console.Write(" > ");

				input = Console.ReadLine();
				Console.Clear();

				string[] n;
				DateTime dtCapture;

				switch (input.Trim())
				{
					case "":
						dtCapture = DateTime.UtcNow;

						bool dateTemp = earthDate;
						earthDate = true;

						eYear = dtCapture.Year;
						eMonth = dtCapture.Month;
						eDay = dtCapture.Day;
						eHour = dtCapture.Hour;
						eMin = dtCapture.Minute;
						eSec = dtCapture.Second;
						eMil = dtCapture.Millisecond;

						WriteDates();

						earthDate = dateTemp;
						break;
					case "n":
						FormatWrite("Press 1 for the standard Darian nomenclature.");
						FormatWrite("Press 2 for the Darian Defrost nomenclature.");
						FormatWrite("Press 3 for the standard Utopian nomenclature.");

						ConsoleKeyInfo cInput = Console.ReadKey();
						List<string> nomenclatures = ["1", "2", "3"];

						InSolNomen = cInput.KeyChar.ToString();
						if (!nomenclatures.Contains(InSolNomen))
							FormatWrite("Invalid selection.");
						else
							Console.Clear();

						break;
					case "r":
						bool _RUNNING = true;
						dateTemp = earthDate;
						earthDate = true;

						while (_RUNNING)
						{
							dtCapture = DateTime.UtcNow;

							while (DateTime.UtcNow.Second == dtCapture.Second && DateTime.UtcNow.Millisecond < dtCapture.Millisecond + 100)
								if (Console.KeyAvailable)
									_RUNNING = false;

							dtCapture = DateTime.UtcNow;

							eYear = dtCapture.Year;
							eMonth = dtCapture.Month;
							eDay = dtCapture.Day;
							eHour = dtCapture.Hour;
							eMin = dtCapture.Minute;
							eSec = dtCapture.Second;
							eMil = dtCapture.Millisecond;

							WriteDates();

							FormatWrite("Press any key to stop the clock.\n");
						}

						earthDate = dateTemp;
						break;
					case "s":
						earthDate = !earthDate;
						break;
					default:
						n = input.Split(' ');

						if (n.Length == 6)
						{
							try
							{
								double S = double.Parse(n[2]);
								int m = int.Parse(n[1]);
								int H = int.Parse(n[0]);
								int D = int.Parse(n[3]);
								int M = int.Parse(n[4]);
								int Y = int.Parse(n[5]);

								if (earthDate)
								{
									eYear = Y;
									eMonth = M;
									eDay = D;
									eHour = H;
									eMin = m;
									eSec = (int)Math.Floor(S);
									eMil = (int)(S % 1 * 1000);
								}
								else
								{
									mYear = Y;
									mMonth = M;
									mDay = D;
									mHour = H;
									mMin = m;
									mSec = (int)Math.Floor(S);
									mMil = (int)(S % 1 * 1000);
								}

								WriteDates();
							}
							catch (FormatException)
							{
								FormatWrite("Invalid date format.");
							}
						}
						else
							FormatWrite("Invalid date format.");
						break;
				}
			}
		}
	}
}
