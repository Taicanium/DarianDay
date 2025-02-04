namespace DarianDay
{
	class DarianDay
	{
		static readonly double MARS_TO_EARTH_DAYS = 1.027491251;
		static readonly double EPOCH_OFFSET = 587744.77817;
		static readonly double ROUND_UP_SECOND = 1 / 86400;
		static readonly int[] eDaysTilMonth = [-1, -1, 30, 58, 89, 119, 150, 180, 211, 242, 272, 303, 333];
		static readonly int[] eDaysInMonth = [0, 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
		static readonly string[] mSolNomens = [
			"Sol Solis", "Sol Lunae", "Sol Martis", "Sol Mercurii", "Sol Jovis", "Sol Veneris", "Sol Saturni", // Standard Darian nomenclature
			"Axatisol", "Benasol", "Ciposol", "Domesol", "Erjasol", "Fulisol", "Gavisol", // Darian Defrost nomenclature
			"Sunsol", "Phobosol", "Deimosol", "Terrasol", "Venusol", "Mercurisol", "Jovisol" // Standard Utopian nomenclature
		];

		static int mYear;
		static int mMonth;
		static string? mMonthName;
		static int mDay;
		static int mDayOfWeek;
		static int mHour;
		static int mMin;
		static int mSec;
		static int mMil;

		static string? mSolName;
		static string? eDayName;
		static uint SolNomen = 1;

		static int marsYear;
		static int earthYear;

		static int eYear;
		static int eMonth;
		static string? eMonthName;
		static int eDay;
		static int eDayOfWeek;
		static int eHour;
		static int eMin;
		static int eSec;
		static int eMil;

		static string? ThisDay;
		static string? ThisSol;

		static int LeapDay;

		static bool DateSuccess = true;
		static bool earthDate = true;

		static int cLineCount = 0;

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
						if (tmpLine[j] != ' ')
							continue;

						Console.Write(tmpLine[..j]);
						Console.Write(new string(' ', Console.BufferWidth - Console.CursorLeft));
						Console.SetCursorPosition(0, top + 1);
						top = Console.CursorTop;
						tmpLine = tmpLine[(j + 1)..];
						j = -1;
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

			daysSince += (double)(eHour / 24.0);
			daysSince += (double)(eMin / 1440.0);
			daysSince += (double)(eSec / 86400.0);
			daysSince += (double)(eMil / 86400000.0);

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

			solsSince += (double)(mHour / 24.0);
			solsSince += (double)(mMin / 1440.0);
			solsSince += (double)(mSec / 86400.0);
			solsSince += (double)(mMil / 86400000.0);

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
			if (sC != 0) // century that does not begin with leap day
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
			else // 1338, 1337, 1337, 1337 ...
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
			else // 668, 669
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

			ThisSol = (doI switch
			{
				0 => "Vernal Equinox (nominal date).\n%S160: The first motion picture about Mars, \"A Trip to Mars,\" was released.",
				1 => "%S222: Approximate time setting for \"October 2026: The Million-Year Picnic\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				3 => "%S136: Robert G. Aitken was born, developed the first complete Martian calendar.",
				4 => "%S188: Mars 1962A (Sputnik 29) was launched, but failed to leave Earth orbit.",
				5 => "%S170: Philip K. Dick was born, author of \"Martian Time-Slip.\"",
				6 => "%S162: Edgar Rice Burroughs` \"The Warlord of Mars\" began serialization in \"All-Story\" (date approximate).",
				8 => "%S194: Mars 4 failed to enter Mars orbit, passed Mars. The motion picture \"Conquest of Space\" was released.",
				10 => "%S194: Mars 5 entered Mars orbit.",
				12 => "%S188: Mars 1 was launched. Robert A. Heinlein`s \"Podkayne of Mars\" began serialization in \"If\" (date approximate).",
				14 => "%S184: The motion picture \"Devil Girl From Mars\" was released.",
				15 => "%S188: Mars 1962B (Sputnik 31) was launched, but failed to leave Earth orbit.\n%S215: The Sixteenth International Mars Society Convention opened in Boulder, Colorado.",
				16 => "%S208: A meteorite, designated Dhofar 378, was discovered near Dhofar, Oman. The meteorite was later identified as having originated on Mars.\n%S214: The Eleventh European Mars Convention opened in Neuchatel, Switzerland.",
				18 => "%S019: Two patches on the lower part of the disk of Mars were described by a Neapolitan Jesuit named Father Bartoli.",
				23 => "%S206: US President Bill Clinton abandoned George Bush`s goal of a manned landing on Mars. His National Science and Technology Council`s statement of National Space Policy only mentioned \"a sustained program to support a robotic presence on the surface of Mars by year 2000 for the purposes of scientific research, exploration and technology development.\"",
				24 => "%S161: Edgar Rice Burroughs` \"Under the Moons of Mars\" began serialization in \"All-Story,\" later was published as a novel under the title \"A Princess of Mars\" (date approximate).",
				25 => "%S214: The motion picture \"Interplanetary\" was released.\n%S171: The motion picture \"Just Imagine\" was released.",
				26 => "%S189: Leigh Brackett`s \"Purple Priestess of the Mad Moon\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).",
				28 => "%S154: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 29th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%S207: \"The Founding Convention of the Mars Society\" opened in Boulder, Colorado.",
				30 => "%S207: \"The Founding Declaration of the Mars Society\" was adopted unanimously in Boulder, Colorado.",
				35 => "%S194: Mars 7 passed Mars. Lander missed Mars.",
				37 => "%S202: Contact with Phobos 2 was lost after 58 sols in Mars orbit.",
				38 => "%S194: Mars 6 passed Mars. Contact with the lander was lost before landing.",
				41 => "%S164: Edgar Rice Burroughs` \"A Princess of Mars\" was published by McClurg.",
				42 => "%S153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the seventh of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				46 => "%S145: Giovanni Schiaparelli recorded the first instance of gemination. \"Great was my astonishment on January 19, when, on examining the Jamuna... I saw instead of its usual appearance two straight and equal parallel lines running between the Niliacus Lacus and Aurorae Sinus. At first I believed this to be the deception of a tired eye, or perhaps the effect of some kind of strabismus, but I soon convinced myself that the phenomenon was real.\"",
				47 => "%S211: Mars Reconnaissance Orbiter entered Mars orbit.",
				50 => "%S191: NASA terminated the contract to procure additional Saturn Vs, ending production with Saturn 515, and abandoning the heavy-lift launch capability required to launch piloted Mars missions.\n%S215: The motion picture \"The Last Days on Mars\" was released.",
				52 => "%S196: A meteorite, designated ALHA 77005, was discovered in the Allan Hills, Antarctica. In the 200s, the meteorite was identified as having originated on Mars.",
				54 => "%S161: Leigh Brackett was born, author of the Eric John Stark series of Martian stories.",
				55 => "%S189: \"The Outer Limits\" episode \"The Invisible Enemy\" aired.",
				59 => "%S171: The motion picture \"Mars\" was released.\n%S189: Mariner 3 failed during launch.\n%S215: The Thirteeth Australian Mars Exploration Conference opened in Sydney, New South Wales.",
				60 => "%S208: A meteorite, designated Sayh al Uhaymir 051, was discovered near Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.",
				63 => "%S213: The motion picture \"Princess of Mars\" was released.",
				65 => "%S153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the eighth of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				66 => "%S189: The motion picture \"Pajama Party\" (also titled \"The Maid and the Martian\") was released.\n%S205: A meteorite, designated QUE 94201, was discovered in the Queen Alexandra Range, Antarctica. The meteorite was later identified as having originated on Mars.",
				70 => "%S206: Mars Global Surveyor was launched.\n%S208: \"The Third International Mars Society Convention\" opened in Toronto, Ontario.",
				71 => "%S165: Edgar Rice Burroughs` \"The Warlord of Mars\" was published by McClurg.",
				72 => "%S214: Mars Science Laboratory \"Curiosity\" was launched.",
				74 => "%S171: Wernher von Braun was born, developed the Saturn V launch vehicle, author of \"The Mars Project.\"",
				78 => "%S206: The motion picture \"Space Jam\" starring Marvin the Martian was released.",
				79 => "%S206: Mars 96 failed during launch.",
				80 => "%S208: The Second International Conference on Mars Polar Science and Exploration opened in Reykjavik, Iceland.",
				81 => "%S189: Mariner 4 was launched.\n%S206: The third annual \"Lunar and Mars Exploration\" conference was held in San Diego, California.",
				82 => "%S205: Jerry Oltion`s \"The Great Martian Pyramid Hoax\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).",
				83 => "%S127: Kurd Lasswitz was born, author of \"Two Planets.\"\n%S153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the ninth of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%S189: Zond 2 was launched.\n%S209: The Second Australian Mars Exploration Conference opened in Sydney, New South Wales.",
				84 => "%S215: The Thirteenth European Mars Convention opened in Paris, France.",
				88 => "%S173: Edgar Rice Burroughs` \"The Swords of Mars\" began serialization in \"Blue Book\" (date approximate).",
				90 => "%S183: The motion picture \"The War of the Worlds\" was released.",
				92 => "%S183: The motion picture \"The War of the Worlds\" was released.",
				93 => "%S045: Christiaan Huygens drew his last sketch of Mars.",
				94 => "%S182: \"Tales of Tomorrow\" episode \"The Crystal Egg\" aired.\n%S207: The First International Conference on Mars Polar Science and Exploration opened in Houston, Texas.",
				95 => "%S173: Carl Sagan was born, first president of the Planetary Society.\n%S215: Mars Orbiter Mission (MOM), also called Mangalyaan, was launched.",
				96 => "%S206: Mars Pathfinder was launched.",
				98 => "%S198: The Third International Colloquium on Mars opened in Pasadena, California.\n%S209: Alex Irvine`s \"Pictures from an Expedition\" was published in \"In The Magazine of Fantasy and Science Fiction.\"",
				100 => "%S153: H. G. Wells` \"The War of the Worlds\" began serialization in England in \"Pearson`s\" (date approximate).",
				103 => "%S209: Approximate time setting for \"August 2002: Night Meeting\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				104 => "%S201: \"The Case For Mars III\" conference opened in Boulder, Colorado.\n%S2013: US President Barack Obama canceled the Constellation program of human expeditions to the Moon and Mars.",
				106 => "%S206: The motion picture \"Mars Attacks!\" was released.",
				107 => "%S164: Arthur C. Clarke was born, author of \"The Sands of Mars.\"\n%S208: \"The Fifth International Mars Society Convention\" opened in Boulder, Colorado.",
				108 => "%S215: Mars Atmosphere and Volatile EvolutioN Mission (MAVEN) was launched.",
				109 => "%S197: A meteorite, designated EETA 79001, was discovered in the Elephant Moraine, Antarctica. The meteorite was later identified as having originated on Mars.",
				114 => "%S154: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 30th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				115 => "%S184: The motion picture \"Devil Girl From Mars\" was released.\n%S188: The motion picture \"The Day Mars Invaded Earth\" was released.\n%S203: \"America at the Threshold: America`s Space Exploration Initiative\" (\"The Stafford Report\") called for manned missions to Mars beginning in 215.",
				117 => "%S152: \"The San Francisco Chronicle\" reported that an observer had read the name of the Almighty in Hebrew letters on the surface of Mars.",
				118 => "%S176: Edgar Rice Burroughs began writing \"John Carter and the Pits of Horz,\" first of a series for new book. The story was later was published under the title \"The City of Mummies.\"",
				122 => "%S208: Kim Stanley Robinson`s \"The Martians\" was published by Spectra.",
				123 => "%S182: The motion picture \"Flight to Mars\" was released.\n%S197: The first episode of the miniseries \"The Martian Chronicles\" aired.",
				124 => "%S197: The second episode of the miniseries \"The Martian Chronicles\" aired.",
				125 => "%S197: The third and final episode of the miniseries \"The Martian Chronicles\" aired.",
				129 => "%S153: H. G. Wells` \"The War of the Worlds\" began serialization in the USA in \"Cosmopolitan\" (date approximate).",
				134 => "%S154: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 31st of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				135 => "%S206: Marvin the Martian appeared in \"The Springfield Files\" episode of the animated television series \"The Simpsons.\"",
				140 => "%S154: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 32nd of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%S207: A meteorite, designated Yamato 980459, was discovered in the Yamato Mountains, Antarctica. The meteorite was later identified as having originated on Mars. \"The Outer Limits\" episode \"Phobos Rising\" aired.",
				142 => "%S154: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 33rd of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				143 => "%S213: Contact with Mars Exploration Rover A (Spirit) was lost.",
				144 => "%S207: Mars Climate Orbiter was launched.\n%S210: The Fourth Australian Mars Exploration Conference opened in Adelaide, South Australia.",
				145 => "%S181: Japanese astronomer Sadao Saeki saw a huge explosion on Mars which produced a mushroom cloud 1450 km in diameter \"like the terrific explosion of a volcano.\" No other people observed this explosion.",
				148 => "%S188: Contact with Mars 1 was lost enroute to Mars.",
				149 => "%S202: US President George H. W. Bush launched the Space Exploration Initiative from the steps of the National Air & Space Museum on the 20th anniversary of the Apollo 11 moon landing, calling for \"a journey into tomorrow, a journey to another planet - a manned mission to Mars.\"",
				151 => "Aphelion (nominal date).\n%S153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 10th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				156 => "%S204: NASA issued \"Mars Design Reference Mission 1.0\" (date approximate).",
				158 => "%S209: The Second European Mars Convention opened in Rotterdam, Netherlands.",
				159 => "%S208: The motion picture \"Red Planet\" was released.",
				162 => "%S209: Approximate time setting for \"October 2002: The Shore\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				163 => "%S210: \"The Seventh International Mars Society Convention\" opened in Chicago, Illinois.",
				164 => "%S165: Isaac Asimov was born, author of \"The Martian Way.\"\n%S212: Phoenix landed in Green Valley, Vastitas Borealis.",
				166 => "%S214: The motion picture \"We Are One\" was released.",
				167 => "%S207: Mars Polar Lander was launched. Approximate time setting for \"January 1999: Rocket Summer\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				169 => "%S209: A meteorite, designated Sayh al Uhaymir 150, was discovered near Dar al Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.",
				170 => "%S202: \"Mars is essentially in the same orbit. Mars is somewhat the same distance from the sun, which is very important. We have seen pictures where there are canals, we believe, and water. If there is water, that means there is oxygen. If oxygen, that means we can breathe.\" -- US Vice President Dan Quayle",
				171 => "%S171: The Mercury Theater of the Air, starring Orson Welles, performed Howard Koch`s radio play \"Invasion from Mars.\" The broadcast terrorized the eastern USA.\n%S213: The First International Conference on Mars Sedimentology and Stratigraphy opened in El Paso, Texas.",
				174 => "%S163: Edgar Rice Burroughs` \"Thuvia, Maid of Mars\" began serialization in \"All-Story Weekly.\"\n%S214: The motion picture \"John Carter\" was released.",
				176 => "%S153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 11th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				177 => "%S195: Viking 1 entered Mars orbit.\n%S208: A meteorite, designated Y000593, was discovered in the Yamato Mountains, Antarctica. The meteorite was later identified as having originated on Mars.",
				179 => "%S175: The motion picture \"Mars Attacks the World\" was released.",
				181 => "%S204: \"The Case For Mars V\" conference opened in Boulder, Colorado.\n%S208: A meteorite, designated Y000749, was discovered in the Yamato Mountains, Antarctica. The meteorite was later identified as having originated on Mars.",
				182 => "%S176: Edgar Rice Burroughs began writing \"The Black Pirates of Barsoom,\" part 2 of a new Mars series.",
				185 => "%S153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 12th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				188 => "%S188: The television series \"My Favorite Martian\" ended with the 107th episode.",
				189 => "%S117: Asaph Hall was born, discoverer of Phobos and Deimos.\n%S211: The Ninth International Mars Society Convention opened in Washington, DC.",
				190 => "%S195: Lin Carter`s \"The Martian El Dorado of Parker Wintley\" was published in \"The DAW Science Fiction Reader\" (date approximate).",
				193 => "Summer Solstice (nominal date).",
				195 => "%S157: \"The New York Times\" reported that Nikola Tesla might use an oscillator to \"wake up\" Mars.",
				196 => "%S197: Contact with Viking Lander 2 was lost after 1,280 sols in Utopia Planitia.",
				197 => "%S184: Fredric Brown`s \"Martians, Go Home\" was published (date approximate).\n%S207: Approximate time setting for \"February 1999: Ylla\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				199 => "%S205: Kim Stanley Robinson`s \"Green Mars\" was published by Spectra.",
				202 => "%S212: The Eighth Australian Mars Exploration Conference opened in Adelaide, South Australia.",
				204 => "%S154: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the last of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				205 => "%S207: The motion picture \"My Favorite Martian\" was released.",
				208 => "%S176: Edgar Rice Burroughs began writing \"Escape on Mars,\" part 3 of a new Mars series. The story was later was published under the title \"The Yellow Men of Mars.\"\nOn this date in 195: Viking Lander 1 achieved first successful landing on Mars in Chryse Planitia. The spacecraft was later renamed the Thomas A. Mutch Memorial Station.\n%S209: A meteorite, designated Sayh al Uhaymir 120, was discovered near Dar al Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.",
				209 => "%S171: Edgar Rice Burroughs` \"A Fighting Man of Mars\" was published by \"Metropolitan.\"",
				211 => "%S210: The Fourth European Mars Convention opened in Milton Keynes, England.",
				213 => "%S195: Viking Orbiter 1 frame 35A72 imaged Cydonia Face and Pyramids.",
				215 => "%S147: Henri Perrotin and Louis Thollon confirmed the existence of the canali.",
				219 => "%S193: Contact with Mariner 9 was lost after 338 sols in Mars orbit.",
				220 => "%S188: The first \"Symposium on the Exploration of Mars\" opened in Denver, Colorado. This was the first conference devoted to the exploration of Mars via spacecraft.",
				222 => "%S195: Viking Orbiter 1 frames 43A01 through 43A04 imaged Deuteronilus Crater Pyramid.",
				225 => "%S195: Viking 2 entered Mars orbit.",
				228 => "%S183: I. M. Levitt`s Earth-Mars Clock debuted in New York, New York.",
				232 => "%S176: Edgar Rice Burroughs began writing \"The Invisible Men of Mars,\" part 4 of a new Mars series.",
				233 => "%S136: A meteorite was observed to fall near Shergotty, India. In the 1980s, the Shergotty meteorite was identified as having originated on Mars.",
				234 => "%S152: \"The observations of 1894 have made it practically certain that the so-called `canals` of Mars are real, whatever may be their explanation,\" Princeton astronomer Charles A. Young declared in his article for \"Cosmopolitan,\" \"Mr. Lowell`s Theory of Mars\" (date approximate).\n%S158: In a letter to \"The New York Times,\" Nikola Tesla declared, \"I can easily bridge the gulf which separates us from Mars.\"\n%S162: Edgar Rice Burroughs began writing \"The Gods of Mars.\"",
				236 => "%S188: Mars 1 passed Mars three months after contact was lost.\n%S206: \"Workshop on Early Mars\" opened in Houston, Texas.",
				238 => "%S175: Edgar Rice Burroughs` \"The Synthetic Men of Mars\" was published in \"Argosy.\"",
				239 => "%S153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 13th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				241 => "%S185: Isaac Asimov`s \"I`m in Marsport without Hilda\" appeared in \"Venture Science Fiction\" (date approximate).",
				242 => "%S212: The Eleventh International Mars Society Convention opened in Boulder, Colorado.",
				244 => "%S214: The Third International Conference on Early Mars opened in Lake Tahoe, Nevada.",
				246 => "%S205: The \"Mars Together\" conference opened in Palo Alto, California.\n%S208: A meteorite, designated Sayh al Uhaymir 094, was discovered near Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.\n%S211: The Fourth International Conference on Mars Polar Science and Exploration opened in Davos, Switzerland.",
				247 => "%S181: Ray Bradbury`s \"The Martian Chronicles\" was published (date approximate).",
				248 => "%S195: Viking Orbiter 1 frame 70A13 imaged the Cydonia Face and Pyramids.",
				249 => "%S213: The Tenth Australian Mars Exploration Conference opened in Melbourne, Victoria.",
				251 => "%S195: Viking Lander 2 landed in Utopia Planitia.",
				252 => "%S191: Mariner 6 was launched.",
				253 => "%S166: Edgar Rice Burroughs` \"The Chessmen of Mars\" began serialization in \"Argosy All-Story Weekly.\"",
				254 => "%S196: Contact with Viking Orbiter 2 was lost after 697 sols in Mars orbit.",
				257 => "%S211: The Sixth Australian Mars Exploration Conference opened in Melbourne, Victoria.",
				258 => "%S215: The Second Humans to Mars Summit opened in Washington, DC.",
				260 => "%S153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 14th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%S182: Leigh Brackett`s \"The Last Days of Shandakor\" appeared in \"Startling Stories\" (date approximate).",
				264 => "%S211: The Sixth European Mars Convention opened in Paris, France.",
				266 => "%S182: Ray Bradbury`s \"The Wilderness\" was published in \"The Philadelphia Enquirer.\"\n%S195: Viking Orbiter 1 frame 86A10 imaged the Utopia Faces.\n%S204: Contact with Mars Observer was lost enroute to Mars.\n%S208: Thomas W. Cronin`s \"As it Is on Mars\" was published.",
				269 => "%S202: NASA`s \"Report of the 90-Day Study on Human Exploration of the Moon and Mars\" estimated the cost of a manned Mars program at $450 billion. Political support for manned Mars missions subsequently collapsed.\n%S204: Mars Observer passed Mars three days after contact was lost.",
				272 => "%S206: Kim Stanley Robinson`s \"Blue Mars\" was published by Spectra.\n%S211: The motion picture \"Fascisti su Marte\" was released.",
				274 => "%S176: Edgar Rice Burroughs` \"John Carter and the Giant of Mars\" appeared in \"Amazing Stories\" (date approximate). The story was later included in the anthology \"John Carter of Mars.\"",
				276 => "%S204: Sailor Mars first appeared in the manga \"Run Run.\"\n%S213: The Thirteenth International Mars Society Convention opened in Dayton, Ohio.",
				278 => "%S188: Philip K. Dick`s \"All We Marsmen\" began serialization in \"Worlds of Tomorrow\" (date approximate).",
				279 => "%S181: The motion picture \"Rocketship X-M\" was released.\n%S191: Mariner 7 was launched.",
				281 => "%S191: Mars 1969A failed during launch.",
				282 => "%S209: Approximate time setting for \"February 2003: Interim\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				286 => "%S195: \"Today we have touched Mars. There is life on Mars and it is us--extensions of our eyes in all directions, extensions of our mind, extensions of our heart and soul have touched Mars today. That`s the message to look for there: We are on Mars. We are the Martians!\" --Ray Bradbury, speaking at \"The Search for Life in the Solar System\" symposium in Pasadena, California.",
				287 => "%S153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 15th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%S180: Marvin the Martian first appeared in animated short film \"Haredevil Hare.\"\n%S184: Robert A. Heinlein`s \"Double Star\" began serialization in \"Astounding Science Fiction\" (date approximate).",
				294 => "%S152: Percival Lowell`s \"Mars\" was published (date approximate).\n%S183: The motion picture \"Mars and Beyond\" was released.",
				295 => "%S194: Lin Carter`s \"The Valley Where Time Stood Still\" was published by DAW (date approximate).",
				299 => "%S191: Mars 1969B failed during launch.",
				300 => "%S206: NASA issued \"Mars Design Reference Mission 3.0\" (date approximate). William K. Hartmann`s \"Mars Underground\" was published (date approximate).",
				301 => "%S153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 16th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				303 => "%S206: Mars Pathfinder landed in Ares Vallis. The spacecraft was later renamed the Carl Sagan Memorial Station.\n%S208: Mars Odyssey was launched.",
				304 => "%S189: Mariner 4 achieved the first flyby of Mars, transmitted 22 images of a cratered surface.\n%S212: The Eighth European Mars Convention opened in Antwerp, Belgium.",
				305 => "%S186: The motion picture \"The Angry Red Planet\" was released.\n%S204: Kim Stanley Robinson`s \"Red Mars\" was published by Spectra.\n%S206: Sojourner, the first roving vehicle on Mars, deployed from the Carl Sagan Memorial Station.",
				309 => "%S209: Thomas W. Cronin`s \"Give Us This Mars\" was published.",
				310 => "%S197: Contact with Viking Orbiter 1 was lost after 1,469 sols in Mars orbit.",
				311 => "%S132: Angelo Secchi described \"a large, triangular patch.\" Then known as the Hourglass Sea, Secchi instead called it the Canale Atlantico, the first use of the word canale applied to Mars. The feature was later named Syrtis Major by Giovanni Schiaparelli.",
				315 => "%S212: Contact with Phoenix lander was lost.",
				316 => "%S214: The Fifteenth International Mars Society Convention opened in Pasadena California.",
				319 => "%S202: The Eighth International Conference on Mars opened in Tucson, Arizona.\n%S214: Mars Science Laboratory \"Curiosity\" landed in Aeolis Palus (\"Bradbury Landing\") in Gale Crater.",
				326 => "%S189: Zond 2 passed Mars four months after contact was lost.",
				332 => "%S176: Edgar Rice Burroughs` \"The City of Mummies\" appeared in \"Amazing Stories\" (date approximate). The story was later included in the anthology \"Llana of Gathol.\"",
				334 => "%S199: Lin Carter`s \"Down to a Sunless Sea\" was published by DAW (date approximate).",
				335 => "%S153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 17th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%S188: The television series \"My Favorite Martian\" debuted.\n%S205: Sailor Mars first appeared in \"An Uncharmed Life\" episode of the animated television series \"Sailor Moon\" (USA air date).",
				336 => "%S208: The First Australian Mars Exploration Conference opened in Melbourne, Victoria.",
				339 => "%S209: Approximate time setting for \"April 2003: The Musicians\" in Ray Bradbury`s \"The Martian Chronicles.\"\n%S215: The Eighth International Conference on Mars opened in Pasadena, California.",
				341 => "%S193: Lin Carter`s \"The Man Who Loved Mars\" was published (date approximate).\n%S213: US President Obama signed the NASA Authorization Act of 2010, which authorized a three-year funding commitment for continued development of the Orion spacecraft and the development of a new heavy-lift Space Launch System to support eventual manned lunar and interplanetary missions; however, the pace of these programs was slowed considerably in comparison to the canceled Constellation program.",
				347 => "%S177: Edgar Rice Burroughs` \"The Skeleton Men of Jupiter\" appeared in \"Amazing Stories\" (date approximate). Later included in the anthology \"John Carter of Mars.\"",
				351 => "%S158: The Martians launched the first of ten cylinders toward Earth, beginning the War of the Worlds.\n%S213: The Tenth European Mars Convention opened in Warsaw, Poland.",
				352 => "%S158: During the War of the Worlds, the Martians launched the second of ten cylinders toward Earth.\n%S210: Thomas W. Cronin`s \"Glory Be to Mars\" was published.",
				353 => "%S158: During the War of the Worlds, the Martians launched the third of ten cylinders toward Earth.",
				354 => "%S158: During the War of the Worlds, the Martians launched the fourth of ten cylinders toward Earth.",
				355 => "%S158: During the War of the Worlds, the Martians launched the fifth of ten cylinders toward Earth.",
				356 => "%S152: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the first of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%S158: During the War of the Worlds, the Martians launched the sixth of ten cylinders toward Earth.\n%S189: \"I say let`s do it quickly and establish a foothold on a new planet while we still have one left to take off from.\" --Wernher von Braun, \"Manned Mars Landing,\" \"Aviation Week & Space Technology.\"\n%S208: Approximate time setting for \"June 2001: And the Moon Be Still as Bright\" in Ray Bradbury`s \"The Martian Chronicles.\"\n%S214: The Twelfth Australian Mars Exploration Conference opened in Canberra, ACT.",
				357 => "%S158: During the War of the Worlds, the Martians launched the seventh of ten cylinders toward Earth.",
				358 => "%S158: During the War of the Worlds, the Martians launched the eighth of ten cylinders toward Earth.\n%S187: NASA`s Office of Manned Space Flight published its \"Long Range Plan,\" which projected the launch of a manned Mars flyby mission in 192 and a manned Mars landing mission in 194.",
				359 => "%S158: During the War of the Worlds, the Martians launched the ninth of ten cylinders toward Earth.",
				360 => "%S158: During the War of the Worlds, the Martians launched the last of ten cylinders toward Earth.\n%S207: The Fifth International Conference on Mars opened in Pasadena, California.",
				363 => "%S192: Mariner 8 failed during launch.\n%S215: The Seventeenth International Mars Society Convention opened in League City, Texas.",
				364 => "%S190: NASA Manned Spacecraft Center issued \"Request for Proposal No. BG721-28-7-528P, Planetary Surface Sample Return Probe Study for Manned Mars/Venus Reconnaissance/Retrieval Missions\" in the 1975-1982 time frame. Rep. Joseph Karth, acting chairman of the House Subcommittee on NASA Oversight, who had been fighting an uphill battle to preserve Project Voyager funding, later expressed his exasperation, for the move cast the program of Saturn V-launched unmanned orbiters and landers in the role of a foot in the door for manned follow-on missions to the planets: \"Very bluntly, a manned mission to Mars or Venus by 1975 or 1977 [194 or 195] is now and always has been out of the question, and anyone who persists in this kind of misallocation of resources at this time will be stopped.\"",
				365 => "%S192: Kosmos 419 was launched, but failed to leave Earth orbit.",
				367 => "%S153: H. G. Wells` \"The War of the Worlds\" was published as a novella (date approximate).",
				368 => "%S188: Roger Zelazny`s \"A Rose for Ecclesiastes\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).\n%S199: \"The Case For Mars II\" conference opened in Boulder, Colorado.",
				369 => "%S195: Gordon Eklund and Gregory Benford`s \"Hellas is Florida\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).",
				371 => "%S206: Mars Global Surveyor entered Mars orbit.",
				372 => "Autumnal Equinox (nominal date).",
				373 => "%S152: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the second of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%S207: Approximate time setting for \"August 1999: The Summer Night\" and \"August 1999: The Earth Men\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				374 => "%S192: Mars 2 was launched.",
				377 => "%S190: U.S. Congress deleted funding for Voyager, a program of Saturn V-launched unmanned orbiters and landers, then scheduled for its first missions in 1973. The program was later down-scoped and renamed \"Viking.\"\n%S200: The motion picture \"Invaders From Mars\" was released.",
				381 => "%S208: A meteorite, designated Sayh al Uhaymir 060, was discovered near Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.",
				382 => "%S153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 18th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%S158: During the War of the Worlds, the first Martian cylinder landed on Horsell Common, east of Horsell.The Deputation advanced upon the pit, waving a white flag. There was a flash of light, an invisible ray of heat flashed from man to man, and each burst into flame, killing about 40 people.\n%S207: \"The Second International Mars Society Convention\" opened in Boulder, Colorado.\n%S210: Approximate time setting for \"April 2005: Usher II\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				383 => "%S158: During the War of the Worlds, the second Martian cylinder landed on the Addlestone Golf Links. A battalion of the Cardigan Regiment rushed the Horsell pit in skirmish order and was annihilated by the Heat-Ray. The fighting-machine then destroyed Woking.\n%S192: Mars 3 was launched.",
				384 => "%S158: During the War of the Worlds, the third Martian cylinder landed north of Pyrford, completing the Surrey Triangle. Five Martian fighting-machines advanced down the Wey River to the confluence with the Thames. Royal Army batteries engaged the Martians, destroying one fighting-machine, but Weybridge and Chertsey were destroyed by Heat-Ray. Later, the St. George's Hill battery damaged one fighting-machine, but was then destroyed. Seven Martian fighting-machines fanned out along a curved line between St. George's Hill, Weybridge, and the village of Send. The Martians discharged Black Smoke across the valley of the Thames, advancing through Street Cobham and Ditton. Richmond, Kingston, and Wimbledon were destroyed.\n%S214: The Twelfth European Mars Convention opened in Munich, Germany.",
				385 => "%S158: During the War of the Worlds, the fourth Martian cylinder landed in Bushey Park, beginning the West End Triangle. The Martians advanced in a line from Hanwell in the north to Coombe and Malden in the south. Organized resistance by the British forces collapsed. The Martians went to and fro over the North Downs between Guildford and Maidstone, using the Black Smoke to eliminate any artillery batteries located there. Police organization in London broke down. The railway system collapsed.\n%S192: Mariner 9 was launched.",
				386 => "%S158: During the War of the Worlds, the fifth Martian cylinder landed in Sheen, and the sixth Martian cylinder landed in Wimbledon, completing the West End Triangle.",
				387 => "%S158: During the War of the Worlds, the seventh Martian cylinder landed in Primrose Hill, where the invaders established their new headquarters. \"HMS Thunder Child\" made a suicide run at three fighting-machines the mouth of the Blackwater to cover the escape of passenger vessels. Two fighting-machines were destroyed. \"Thunder Child\" was also destroyed.",
				388 => "%S158: During the War of the Worlds, the eighth Martian cylinder landed (unreported). A fighting-machine destroyed Leatherhead, with every soul in it.",
				389 => "%S158: During the War of the Worlds, the ninth Martian cylinder landed (unreported). The Martians vacated the Sheen cylinder except for one fighting-machine and one handling-machine.",
				390 => "%S158: During the War of the Worlds, the last of ten Martian cylinder landed (unreported).",
				391 => "%S153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 19th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%S165: Ray Bradbury was born, author of \"The Martian Chronicles.\"\n%S169: Edgar Rice Burroughs began dictating \"A Fighting Man of Mars\" on Ediphone.\n%S215: The Fourteenth European Mars Convention opened in Podzamcze, Poland.",
				392 => "%S207: A meteorite, designated Dar al Gani 975, was discovered near Dar al Gani, Libya. The meteorite was later identified as having originated on Mars.",
				395 => "%S206: Contact with the Carl Sagan Memorial Station (Mars Pathfinder) was lost after 93 sols in Ares Vallis.",
				399 => "%S195: John Varley`s \"In the Hall of the Martian Kings\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).\n%S209: Approximate time setting for \"June 2003: Way in the Middle of the Air\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				400 => "%S206: The motion picture \"RocketMan\" was released.\n%S209: Mars Express was launched.",
				402 => "%S158: The last Martians succumbed to terrestrial disease, ending the War of the Worlds.\n%S201: Soviet President Mikhail Gorbachev called for a joint unmanned Mars mission with the USA.",
				404 => "%S158: Robert A. Heinlein was born, author of \"Red Planet,\" \"Podkayne of Mars,\" and \"Stranger in a Strange Land.\"\n%S159: Nikola Tesla discussed communication with Mars in an article in \"The New York Times.\"\n%S191: Mariner 6 passed Mars, returned 26 near encounter images.",
				407 => "%S214: Mars Atmosphere and Volatile EvolutioN Mission (MAVEN) entered Mars orbit",
				408 => "%S209: Mars Exploration Rover Spirit was launched.",
				409 => "%S191: Mariner 7 passed Mars, returned 33 near encounter images.\n%S214: Mars Orbiter Mission (MOM), also called Mangalyaan, entered Mars orbit.",
				410 => "%S201: The Planetary Society`s \"Mars Declaration\" was published in \"The Washington Post.\"",
				416 => "%S202: The motion picture \"Martians Go Home\" was released.\n%S208: Approximate time setting for \"August 2001: The Settlers\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				417 => "%S153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 20th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				420 => "%S119: Giovanni Schiaparelli was born, developed first detailed maps of Mars and originated the system of areographic nomenclature in use today.\n%S200: The \"NASA Mars Conference\" opened in Washington, DC.",
				421 => "%S176: Edgar Rice Burroughs` \"The Black Pirates of Barsoom\" appeared in \"Amazing Stories\" (date approximate). The story was later included in the anthology \"Llana of Gathol.\"\n%S208: \"The Infinite Worlds of H.G. Wells\" episode \"The Crystal Egg\" aired.",
				422 => "%S203: US President George H. W. Bush issued National Space Policy Directive 6, \"Space Exploration Initiative,\" calling for human expeditions to Mars.",
				423 => "%S207: Two meteorites, designated Los Angeles 001 and 002, were discovered in the Mojave Desert near Los Angeles, California. The meteorites were later identified as having originated on Mars.\n%S210: \"Star Trek: Enterprise\" episode \"Terra Prime\" aired.",
				424 => "%S196: The Second International Colloquium on Mars opened in Pasadena, California.",
				428 => "%S202: Voicing his opposition to the Bush Administration`s Space Exploration Initiative, Senator Albert Gore, Jr. told his fellow legislators, \"Before discussing a mission to Mars, the Administration needs a mission to reality.\"",
				429 => "%S151: The Lowell Observatory began its study of Mars. Eighteen months later, Percival Lowell was published the observations of the 151 opposition in a popular book, Mars.",
				431 => "%S091: William Herschel calculated a rotational period for Mars of 24 hours, 39 minutes, and 21.67 seconds. He also confirmed that the north polar spot was eccentric to the pole.",
				434 => "%S209: Mars Exploration Rover Opportunity was launched.",
				435 => "%S153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 21st of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%S191: A NASA report to US President Richard Nixon`s Space Task Group, which was chartered to explore options for space efforts beyond the Apollo program, stated, \"Manned expeditions to Mars could begin as early as 1981.\"",
				436 => "%S202: George Bush became the first US President to set a target date for sending humans to Mars: \"I believe that before America celebrates the 50th anniversary of its landing on the Moon, the American flag should be planted on Mars.\"",
				437 => "%S208: \"The Fourth International Mars Society Convention\" opened at Stanford University, California.",
				439 => "%S188: \"The Outer Limits\" episode \"Controlled Experiment\" aired.",
				440 => "%S015: Two years after making his first drawing of Mars, Neapolitan lawyer and amateur astronomer Francesco Fontana produced a second drawing, which like the first, featured a dark spot, which has been attributed to an optical defect in his telescope.",
				445 => "%S210: The motion picture \"Crimson Force\" was released.",
				447 => "%S176: Edgar Rice Burroughs` \"The Black Pirates of Barsoom\" was published in Amazing Stories.\n%S198: Contact with the Thomas A. Mutch Memorial Station (Viking Lander 1) was lost after 2,244 sols in Chryse Planitia.\\n%S209: The Sixth International Conference on Mars opened in Pasadena, California.",
				449 => "%S158: I. M. Levitt was born, inventor of a Martian calendar and the first Earth-Mars mechanical date-time computer.\n%S191: The report by US President Richard Nixon`s Space Task Group, \"The Post-Apollo Space Program: Directions for the Future,\" stated, \"We conclude that NASA has the demonstrated organizational competence and technology base, by virtue of the Apollo success and other achievements, to carry out a successful program to land man on Mars within 15 years.\" Nevertheless, the report backed away from an early manned landing on Mars, recommending that the focus for the next decades in space should be on the development of hardware and systems that would ultimately support a manned mission to Mars at the close of the 20th century.",
				450 => "%S201: Phobos 1 was launched.",
				451 => "%S141: Edgar Rice Burroughs was born, author of the \"Barsoom\" series.",
				455 => "%S010: Christiaan Huygens was born, drew the first identifiable Martian surface feature.\n%S201: Phobos 2 was launched.",
				457 => "%S202: The motion picture \"Total Recall\" was released.",
				458 => "%S180: Robert A. Heinlein`s \"Red Planet\" was published by Charles Scribner`s Sons.",
				459 => "%S202: \"The Case For Mars IV\" conference opened in Boulder, Colorado.",
				460 => "%S170: Edgar Rice Burroughs` \"A Fighting Man of Mars\" began serialization in \"Blue Book\" (date approximate).",
				464 => "%S214: The motion picture \"Master Race from Mars\" was released.",
				469 => "%S182: Isaac Asimov`s \"The Martian Way\" appeared in \"Galaxy Science Fiction\" (date approximate).\n%S210: The motion picture \"The War of the Worlds\" was released.",
				471 => "%S125: Omsby MacKnight Mitchel observed a white patch detached from the south polar cap of Mars. The feature became known as the Mountains of Mitchel. Actually it is a depression.",
				472 => "%S208: The First European Mars Convention opened in Paris, France.",
				473 => "%S142: Asaph Hall gave up a search for Martian moons. The following night, having resumed his search at the insistence of his wife, Angelina, he detected a faint object near Mars, which he later named Deimos.",
				474 => "%S142: \"The New York Times\" asked, \"Is Mars inhabited?\" in an editorial. As the best opposition since 1798 approached, questions in the popular mind came to the fore and the possibility of life on Mars was discussed in the press.",
				478 => "%S142: Asaph Hall discovered Phobos.\n%S215: The Orion spacecraft, designed for manned interplanetary missions, was tested successfully in Earth orbit on an unmanned flight.",
				479 => "%S193: Mars 4 was launched.",
				480 => "%S142: Asaph Hall announced the discovery of Mars` two moons. At the suggestion of Henry Madan, the Science Master of Eton, England, Hall named the moons Phobos and Deimos.",
				481 => "%S160: An unfortunate dog in Nakhla, Egypt was struck by part of a meteorite and killed. In the 1980s, the Nakhla meteorite was identified as having originated on Mars.\n%S183: Alfred Coppel`s \"Mars Is Ours\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).",
				482 => "%S176: Edgar Rice Burroughs` \"The Yellow Men of Mars\" appeared in \"Amazing Stories\" (date approximate). The story was later included in the anthology \"Llana of Gathol.\"\n%S184: A dust storm began with a bright cloud over the Hellas-Noachis region that spread to engulf the whole planet by mid-September.",
				483 => "%S193: Mars 5 was launched.\n%S209: In perihelic solar inferior conjunction, Earth made its closest approach to Mars in 32,000 Martian years, coming within 55.8 million kilometers.",
				484 => "%S160: Writing down his daydreams on the backs of old letterheads of previously failed businesses, Edgar Rice Burroughs used free time at his office to begin \"A Princess of Mars\" (date approximate).\n%S203: Sailor Mars first appeared in \"An Uncharmed Life\" episode of the animated television series \"Sailor Moon\" (Japan air date).",
				485 => "Perihelion (nominal date).\n%S207: Two meteorites, designated Sayh al Uhaymir 005 and 008, were discovered near Sayh al Uhaymir, Oman. The meteorites were later identified as having originated on Mars.",
				487 => "%S209: The Third Australian Mars Exploration Conference opened in Perth, Western Australia.",
				490 => "%S142: Henry Draper of New York and Edward Singleton Holden of the U.S. Naval Observatory in Washington claimed to have jointly discovered a third moon of Mars at Draper`s private observatory at Hastings-on-the-Hudson. This discovery proved to be false; in fact, the proposed moon did not even obey Kepler`s laws.\n%S151: The British scientific journal \"Nature\" reported that the Lick and Nice Observatories had seen a great light on Mars, which was later speculated to have been the casting of the huge gun used to launch the War of the Worlds.",
				492 => "%S207: Contact with Mars Polar Lander was lost prior to landing on Mars.\n%S221: Approximate time setting for \"April 2026: The Long Years\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				493 => "%S185: A few months before it was transformed into a space agency by the National Aeronautics and Space Act, a report by the National Advisory Committee on Aeronautics projected that a manned Mars mission might be undertaken in 1977.",
				494 => "%S193: Mars 6 was launched.",
				495 => "%S208: Mars Odyssey entered Mars orbit.",
				497 => "%S193: Mars 7 was launched.",
				501 => "Storm season begins.\n%S210: Approximate time setting for \"August 2005: The Old Ones\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				504 => "%S142: Giovanni Schiaparelli began a detailed study of Mars for the purpose of drawing a new and accurate map of the planet.",
				505 => "%S201: Contact with Phobos 1 was lost en route to Mars.",
				507 => "%S142: Giovanni Schiaparelli observed the canale Cyclops.",
				510 => "%S167: The motion picture \"Aelita: Queen of Mars\" was released.",
				511 => "%S210: The Eighth International Mars Society Convention opened in Boulder, Colorado. Mars Reconnaissance Orbiter was launched.",
				512 => "%S209: The Third European Mars Convention opened in Bremen, Germany.",
				513 => "%S162: Orson Welles was born, leader of the Mercury Theater of the Air, which performed the \"Panic Broadcast.\"\n%S210: \"The Sixth International Mars Society Convention\" opened in Eugene, Oregon.",
				514 => "Winter Solstice (nominal date).\n%S142: Giovanni Schiaparelli observed the canale Ambrosia.",
				515 => "%S188: Philip K. Dick`s \"Martian Time-Slip\" was published (date approximate).",
				518 => "%S210: The Fifth Australian Mars Exploration Conference opened in Canberra, ACT.",
				519 => "%S186: Arthur C. Clarke`s \"Crime on Mars,\" later titled \"The Trouble with Time,\" was published in \"Ellery Queen\" (date approximate).",
				520 => "%S141: Earl C. Slipher was born, observed Mars extensively.\n%S211: The Seventh International Conference on Mars opened in Pasadena, California.",
				523 => "%S211: The Seventh Australian Mars Exploration Conference opened in Perth, Western Australia.",
				525 => "%S142: Giovanni Schiaparelli observed the canali Ganges and Phison.",
				526 => "%S165: Edgar Rice Burroughs began writing \"The Chessmen of Mars.\"",
				527 => "%S130: Percival Lowell was born, developed detailed maps of Mars and believed in an advanced Martian civilization.\n%S196: Robert F. Young`s \"The First Mars Mission\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).",
				529 => "%S203: Michael Cassutt`s \"The Last Mars Trip\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).\n%S209: The Third International Conference on Mars Polar Science and Exploration opened in Lake Louise, Alberta.",
				530 => "%S166: Edgar Rice Burroughs` \"The Chessmen of Mars\" was published by McClurg.\n%S170: Lin Carter was born, author of \"The Man Who Loved Mars.\"",
				531 => "%S142: Giovanni Schiaparelli observed Mare Erythraeum and Noachis to be obscured by clouds.\n%S210: Approximate time setting for \"September 2005: The Martian\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				533 => "%S199: A meteorite, designated ALH 84001, was discovered in the Allan Hills, Antarctica. In 1993, the meteorite was identified as having originated on Mars. In 1996, a team of scientists announced evidence on fossilized Martian life in the meteorite.",
				534 => "%S208: Approximate time setting for \"December 2001: The Green Morning\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				537 => "%S092: William Herschel observed the south polar cap of Mars. \"I am inclined to think that the white spot has some little revolution.... It is rather probable that the real pole, though within the spot, may lie near the circumference of it, or one-third of its diameter from one of the sides. A few days more will show it, as I shall now fix my particular attention on it.\"",
				540 => "%S160: \"Martians Build Two Immense Canals in Two Years\" reported in \"The New York Times.\"\n%S176: Edgar Rice Burroughs` \"The Invisible Men of Mars\" appeared in Amazing Stories (date approximate). The story was later included in the anthology Llana of Gathol.",
				541 => "%S142: Giovanni Schiaparelli observed the canale Eunostos.",
				542 => "%S207: A meteorite, designated Dhofar 019, was discovered near Dhofar, Oman. The meteorite was later identified as having originated on Mars.",
				545 => "%S211: Phoenix lander was launched.",
				547 => "%S173: ERB Inc released \"The Swords of Mars.\"",
				548 => "%S192: Mariner 9 became the first spacecraft to orbit Mars.",
				549 => "%S109: A meteorite was observed to fall near Chassigny, France. In the 1980s, the Chassigny meteorite was identified as having originated on Mars. It was the first Martian meteorite to be discovered.\n%S194: Viking 1 was launched.",
				554 => "%S212: NASA issued \"Human Exploration of Mars Design Reference Architecture 5.0\" (date approximate).",
				558 => "%S189: Philip K. Dick`s \"We Can Remember It for You Wholesale\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate). It was the basis of of the 1990 motion picture \"Total Recall.\"",
				559 => "%S207: A meteorite, designated GRV 99027, was discovered in the Grove Mountains, Antarctica. The meteorite was later identified as having originated on Mars.",
				561 => "%S192: Mariner 9 returned the first image of Deimos. Mars 2 Orbiter entered Mars orbit. Mars 2 Lander impacted on Mars.",
				564 => "%S176: Edgar Rice Burroughs began writing \"The Skeleton Men of Jupiter,\" the first of a planned new John Carter series.\n%S192: Mariner 9 returned the first image of Phobos.",
				565 => "%S209: A meteorite, designated Sayh al Uhaymir 125, was discovered near Dar al Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.",
				566 => "%S192: Mars 3 Orbiter entered Mars orbit. Contact with Mars 2 Lander was lost shortly after landing. No useful data was returned.",
				567 => "%S178: The motion picture \"The Purple Monster Strikes\" was released.",
				568 => "%S197: \"The Case For Mars I\" conference opened in Boulder, Colorado.",
				569 => "%S212: The Ninth Australian Mars Exploration Conference opened in Adelaide, South Australia.",
				571 => "%S191: An article in \"Time\" magazine speculated that the Charles Manson \"family\" might have been a Martian nest patterned on Robert A. Heinlein`s \"Stranger in a Strange Land.\" The \"family\" had murdered actress Sharon Tate and four others in Los Angeles.\n%S194: Viking 2 was launched.",
				573 => "%S123: Camille Flammarion was born, developed detailed maps of Mars and believed in an advanced civilization on that planet.\n%S211: The Tenth International Mars Society Convention opened in Los Angeles, California.",
				581 => "%S207: Approximate time setting for \"March 2000: The Taxpayer\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				582 => "%S208: A meteorite, designated Sayh al Uhaymir 090, was discovered near Dar al Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.",
				583 => "%S212: The Twelfth International Mars Society Convention opened in College Park, Maryland.",
				585 => "%S214: The First Humans to Mars Summit opened in Washington, DC.",
				586 => "%S182: Lester Del Rey and Erik Van Lhin`s \"Police Your Planet\" began serialization in \"Science Fiction Adventures\" (date approximate).",
				590 => "%S143: Discussing the canali in a letter to Nathaniel Green, Giovanni Schiaparelli declared, \"It is [as] impossible to doubt their existence as that of the Rhine on the surface of the Earth.\"\n%S207: The motion picture \"Mission to Mars\" was released.\n%S209: A meteorite, designated MIL 03346, was discovered in the Miller Range of the Transantarctic Mountains. The meteorite was later identified as having originated on Mars.\n%S210: Approximate time setting for \"November 2005: The Luggage Store,\" \"November 2005: The Off Season,\" and \"November 2005: The Watchers\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				593 => "%S210: The Fifth European Mars Convention opened in Swindon, England.",
				594 => "%S153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 22nd of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				595 => "%S208: Approximate time setting for \"February 2002: The Locusts\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				596 => "%S206: A meteorite, designated Dar al Gani 476, was discovered near Dar al Gani, Libya. The meteorite was later identified as having originated on Mars.",
				599 => "%S201: Soviet President Mikhail Gorbachev called for a joint manned Mars mission with the USA.",
				600 => "%S209: Mars Express entered orbit. Contact with Beagle 2 was lost prior to landing in Isidis Planitia.",
				602 => "%S206: A meteorite, designated Dar al Gani 876, was discovered near Dar al Gani, Libya. The meteorite was later identified as having originated on Mars.",
				603 => "%S193: The First International Colloquium on Mars opened in Pasadena, California.",
				604 => "%S143: Giovanni Schiaparelli discovered a small white patch in Tharsis and named it Nix Olympica. Nearly a century later, Mariner 9 imagery revealed this feature to be the largest mountain in the Solar System, and the feature was renamed Olympus Mons.",
				607 => "%S152: Charles A. Young of Princeton University discussed the question \"Is Mars Inhabited?\" in \"The Boston Herald.\"\n%S153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 23rd of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n",
				610 => "%S209: Mars Exploration Rover Spirit landed in Gusev Crater. The spacecraft was later renamed the Columbia Memorial Station.\n%S221: Approximate time setting for \"August 2026: There Will Come Soft Rains\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				611 => "%S207: Approximate time setting for \"April 2000: The Third Expedition\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				613 => "%S201: A meteorite, designated LEW 88516, was discovered at Lewis Cliff, Antarctica. The meteorite was later identified as having originated on Mars.\n%S201: Percival Lowell reported \"Frost on Mars\" to \"The New York Times.\"\n%S203: Mars Observer was launched.\n%S213: A meteorite, designated Tissint, was observed to fall in Tata Province in the Guelmim-Es Semara region of Morocco. The meteorite was later identified as having originated on Mars.",
				614 => "%S136: H. G. Wells was born, author of \"The War of the Worlds.\"",
				616 => "%S209: Two meteorites, designated Sayh al Uhaymir 130 and 131, were discovered near Dar al Sayh al Uhaymir, Oman. The meteorites were later identified as having originated on Mars.",
				617 => "%S165: The motion picture \"A Message from Mars\" was released.\n%S186: Mars 1960A, the first known Mars mission, failed during launch.\n%S213: The Eleventh Australian Mars Exploration Conference opened in Perth, Western Australia.",
				618 => "%S161: Edgar Rice Burroughs drew a map of Barsoom at the request of his publisher.",
				619 => "%S210: Approximate time setting for \"December 2005: The Silent Towns\" in Ray Bradbury`s \"The Martian Chronicles.\"\n%S211: The Seventh European Mars Convention opened in Delft, Netherlands.",
				620 => "%S188: The motion picture \"Robinson Crusoe on Mars\" was released.\n%S209: US President George W. Bush announced plans to establish a lunar base to support human missions to Mars.",
				621 => "%S182: The motion picture \"Abbott and Costello Go to Mars\" was released.\n%S186: Mars 1960B failed during launch.",
				626 => "%S216: The Third Humans to Mars Summit opened in Washington, DC.",
				629 => "%S174: Edgar Rice Burroughs began writing \"The Synthetic Men of Mars.\"\n%S205: \"The Case for Mars VI\" conference opened in Boulder, Colorado.",
				630 => "%S209: Mars Exploration Rover Opportunity landed in Meridiani Planum.\n%S213: The Fourteenth International Mars Society Convention opened in Dallas, Texas.",
				632 => "%S175: ERB Inc released \"The Synthetic Men of Mars.\"\n%S192: Mariner 9 frame 4205-78 imaged Pyramids of Elysium.",
				635 => "%S195: Lin Carter`s \"The City Outside the World\" was published by Berkeley (date approximate).",
				636 => "%S153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 24th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				637 => "%S182: The motion picture \"Invaders From Mars\" was released.",
				640 => "Storm season ends.\n%S000: Galileo Galilei discovered that, like the Moon, Mars had a phase.",
				643 => "%S026: Christiaan Huygens drew the first sketch of Mars, including a dark, triangular area later named Syrtis Major, the first Martian surface feature identified from Earth. Huygens used his own design of telescope, which was of much higher quality than that of his predecessors and allowed a magnification of 50 times.",
				646 => "%S026: Christiaan Huygens noted, \"The rotation of Mars, like that of the Earth, seems to have a period of 24 hours.\"\n%S153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 25th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%S201: Phobos 1 passed Mars five months after contact was lost.",
				647 => "%S185: The motion picture \"Quatermass and the Pit\" was released.",
				649 => "%S152: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the third of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%S205: The National Science Foundation announced evidence for possible early life on Mars in Antarctic meteorite ALH84001.",
				650 => "%S201: Phobos 2 entered Mars orbit.",
				653 => "%S152: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the fourth of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%S187: A meteorite was observed to fall near Zagami, Nigeria. In the 1980s, this meteorite was identified as having originated on Mars.",
				654 => "%S153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 26th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				657 => "%S212: The Ninth European Mars Convention opened in Bergamo, Italy.",
				659 => "%S153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 27th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				662 => "%S152: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the fifth of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				663 => "%S208: The motion picture \"Stranded\" was released.",
				664 => "%S152: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the sixth of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				665 => "%S153: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 28th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				668 => "%S213: The Fifth International Conference on Mars Polar Science and Exploration opened in Fairbanks, Alaska.",
				_ => string.Empty
			}).Replace("%S", "On this sol in ");

			double MarsSolNum = doI + 1;
			mMonth = new List<int>([28, 56, 84, 112, 140, 167, 195, 223, 251, 279, 307, 334, 362, 390, 418, 446, 474, 501, 529, 557, 585, 613, 641, 669]).FindIndex(n => n >= MarsSolNum) + 1;

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

			var sCD = Math.Floor(d / 146097); // what 400 year span
			var doCD = Math.Floor(d - (sCD * 146097));

			var sC = 0;
			var doC = doCD;
			if (doCD != 0) sC = (int)Math.Floor((doCD - 1) / 36524);
			if (sC != 0) doC -= sC * 36524 + 1;
			var doIV = doC;

			int sIV;
			if (sC != 0) // 1460 + 1461*24
			{
				sIV = (int)Math.Floor((doC + 1) / 1461);
				if (sIV != 0) doIV -= sIV * 1461 - 1;
			}
			else // 1461*25
			{
				sIV = (int)Math.Floor(doC / 1461);
				if (sIV != 0) doIV -= sIV * 1461;
			}

			var sI = 0;
			var doI = doIV;
			if (sC != 0 && sIV == 0) // four 365-day years in a row
			{
				sI = (int)Math.Floor(doIV / 365);
				if (sI != 0) doI -= sI * 365;
			}
			else // normal leap year cycle
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

				if (tmpDayOfYear <= tmpDaysInMonth)
				{
					earthMonth = i;
					break;
				}
				
				tmpDayOfYear -= tmpDaysInMonth;
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

			LeapDay = (
				doI + 1 < 60 && (
					(earthYear % 4 == 0 && earthYear % 100 != 0)
					|| earthYear % 400 == 0)
				) ? 1 : 0;

			ThisDay = ((doI + 1 - LeapDay) switch
			{
				1 => "%D1898: H. G. Wells` \"The War of the Worlds\" was published as a novella (date approximate).\n%D1941: Edgar Rice Burroughs` \"John Carter and the Giant of Mars\" appeared in \"Amazing Stories\" (date approximate). The story was later included in the anthology \"John Carter of Mars.\"\n%D1977: Gordon Eklund and Gregory Benford`s \"Hellas is Florida\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).\n%D1995: Jerry Oltion`s \"The Great Martian Pyramid Hoax\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).\n%D1999: Approximate time setting for \"January 1999: Rocket Summer\" in Ray Bradbury`s \"The Martian Chronicles.\"\n%D2013: The motion picture \"Master Race from Mars\" was released.",
				2 => "%D1920: Isaac Asimov was born, author of \"The Martian Way.\"",
				3 => "%D1999: Mars Polar Lander was launched.",
				4 => "%D2004: Mars Exploration Rover Spirit landed in Gusev Crater. The spacecraft was later renamed the Columbia Memorial Station.",
				7 => "%D1921: Edgar Rice Burroughs began writing \"The Chessmen of Mars.\"\n%D1939: Edgar Rice Burroughs` \"The Synthetic Men of Mars\" was published in \"Argosy.\"",
				10 => "%D1990: The Fourth International Conference on Mars opened in Tucson, Arizona.",
				11 => "%D2004: Two meteorites, designated Sayh al Uhaymir 130 and 131, were discovered near Dar al Sayh al Uhaymir, Oman. The meteorites were later identified as having originated on Mars.",
				12 => "%D1997: Marvin the Martian appeared in \"The Springfield Files\" episode of the animated television series \"The Simpsons.\"",
				13 => "%D1964: \"The Outer Limits\" episode \"Controlled Experiment\" aired.\n%D1907: In a letter to \"The New York Times,\" Nikola Tesla declared, \"I can easily bridge the gulf which separates us from Mars.\"\n%D1980: A meteorite, designated EETA 79001, was discovered in the Elephant Moraine, Antarctica. The meteorite was later identified as having originated on Mars.",
				14 => "%D1954: I. M. Levitt`s Earth-Mars Clock debuted in New York, New York.\n%D2004: US President George W. Bush announced plans to establish a lunar base to support human missions to Mars.",
				15 => "%D1898: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 18th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%D1905: \"The New York Times\" reported that Nikola Tesla might use an oscillator to \"wake up\" Mars.\n%D1949: Robert A. Heinlein`s \"Red Planet\" was published by Charles Scribner`s Sons.\n%D1979: The Second International Colloquium on Mars opened in Pasadena, California.",
				16 => "%D1950: Japanese astronomer Sadao Saeki saw a huge explosion on Mars which produced a mushroom cloud 1450 km in diameter \"like the terrific explosion of a volcano.\" No other people observed this explosion.",
				19 => "%D1882: Giovanni Schiaparelli recorded the first instance of gemination.\"Great was my astonishment on January 19, when, on examining the Jamuna... I saw instead of its usual appearance two straight and equal parallel lines running between the Niliacus Lacus and Aurorae Sinus. At first I believed this to be the deception of a tired eye, or perhaps the effect of some kind of strabismus, but I soon convinced myself that the phenomenon was real.\"\n%D1970: An article in \"Time\" magazine speculated that the Charles Manson \"family\" might have been a Martian nest patterned on Robert A. Heinlein`s \"Stranger in a Strange Land.\" The \"family\" had murdered actress Sharon Tate and four others in Los Angeles.\n%D2002: A meteorite, designated Sayh al Uhaymir 090, was discovered near Dar al Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.",
				24 => "%D2000: A meteorite, designated Dhofar 019, was discovered near Dhofar, Oman. The meteorite was later identified as having originated on Mars.",
				25 => "%D1898: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 19th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%D1989: Phobos 1 passed Mars five months after contact was lost.\n%D2004: Mars Exploration Rover Opportunity landed in Meridiani Planum.",
				27 => "%D1980: The first episode of the miniseries \"The Martian Chronicles\" aired.",
				28 => "%D1980: The second episode of the miniseries \"The Martian Chronicles\" aired.",
				29 => "%D1980: The third and final episode of the miniseries \"The Martian Chronicles\" aired.\n%D1989: Phobos 2 entered Mars orbit.",
				31 => "%D1897: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the seventh of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				32 => "%D1912: Edgar Rice Burroughs` \"Under the Moons of Mars\" began serialization in \"All-Story,\" later was published as a novel under the title \"A Princess of Mars\" (date approximate).\n%D1943: Edgar Rice Burroughs` \"The Skeleton Men of Jupiter\" appeared in \"Amazing Stories\" (date approximate). Later included in the anthology \"John Carter of Mars.\"\n%D1956: Robert A. Heinlein`s \"Double Star\" began serialization in \"Astounding Science Fiction\" (date approximate).\n%D1977: John Varley`s \"In the Hall of the Martian Kings\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).\n%D1999: Approximate time setting for \"February 1999: Ylla\" in Ray Bradbury`s \"The Martian Chronicles.\"\n%D2002: Approximate time setting for \"February 2002: The Locusts\" in Ray Bradbury`s \"The Martian Chronicles.\"\n%D2003: Approximate time setting for \"February 2003: Interim\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				33 => "%D1896: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the first of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				35 => "%D1694: Christiaan Huygens drew his last sketch of Mars.",
				39 => "%D1972: Mariner 9 frame 4205-78 imaged the Pyramids of Elysium.\n%D2000: A meteorite, designated GRV 99027, was discovered in the Grove Mountains, Antarctica. The meteorite was later identified as having originated on Mars.\n%D2001: A meteorite, designated Sayh al Uhaymir 094, was discovered near Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.",
				41 => "%D2010: US President Barack Obama canceled the Constellation program of human expeditions to the Moon and Mars.\n%D1974: Mars 4 failed to enter Mars orbit, passed Mars.",
				43 => "%D1974: Mars 5 entered Mars orbit.\n%D1999: The motion picture \"My Favorite Martian\" was released.",
				45 => "%D1963: The motion picture \"The Day Mars Invaded Earth\" was released.",
				46 => "%D1858: William Henry Pickering was born; with Andrew Ellicott Douglass, he devised the first known Martian calendar to be used in scientific research.\n%D1936: ERB Inc released \"The Swords of Mars.\"\n%D1940: ERB Inc released \"The Synthetic Men of Mars.\"",
				48 => "Quirinalia, sacred to Quirinus, celebrating the ascension of Romulus. Celebrated by the flamen Quirinalis. It is also known as the Feast of Fools.",
				49 => "%D1910: The first motion picture about Mars, \"A Trip to Mars,\" was released.\n%D1922: Edgar Rice Burroughs` \"The Chessmen of Mars\" began serialization in \"Argosy All-Story Weekly.\"",
				51 => "%D1898: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 20th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				55 => "%D1897: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the eighth of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				56 => "%D1969: Mariner 6 was launched.",
				57 => "%D1842: Camille Flammarion was born, developed detailed maps of Mars and believed in an advanced civilization on that planet.",
				58 => "Equirria, a festival of horse racing dedicated to Mars. Established by Romulus himself in the early days of Rome. It is held in the Campus Martius in Roma, or the Campus Martialis if the former is flooded. ",
				59 => "%D1928: Edgar Rice Burroughs began dictating \"A Fighting Man of Mars\" on Ediphone.",
				60 => "Feriae Marti, the original New Year`s Day on the Roman calendar. The Salii, a collegium divided into the Palatini (devoted to Mars Gradivus) and the Agonenses (devoted to Quirinus), dressed in archaic military armor and bearing the sacred shields, dance throughout the streets and chant the ancient Carme Saliare. After their dance, the Salii hold a spectacular feast.\n%D1941: Edgar Rice Burroughs` \"The City of Mummies\" appeared in \"Amazing Stories\" (date approximate). The story was later included in the anthology \"Llana of Gathol.\"\n%D1953: Lester Del Rey and Erik Van Lhin`s \"Police Your Planet\" began serialization in \"Science Fiction Adventures\" (date approximate).\n%D1973: Lin Carter`s \"The Man Who Loved Mars\" was published (date approximate).\n%D2000: Approximate time setting for \"March 2000: The Taxpayer\" in Ray Bradbury`s \"The Martian Chronicles.\n%D2001: Thomas W. Cronin`s \"As it Is on Mars\" was published.\n%D2003: Thomas W. Cronin`s \"Give Us This Mars\" was published.\n%D2005: Thomas W. Cronin`s \"Glory Be to Mars\" was published (date approximate).\n%D2012: The motion picture \"We Are One\" was released.",
				67 => "%D1899: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 30th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				69 => "%D1974: Mars 7 passed Mars. Lander missed Mars.\n%D2006: Mars Reconnaissance Orbiter entered Mars orbit.",
				70 => "%D1898: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 21st of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%D2012: The motion picture \"John Carter\" was released.",
				71 => "%D1974: Mars 6 passed Mars. Contact with the lander was lost before landing.\n%D2000: The motion picture \"Mission to Mars\" was released.",
				72 => "%D1855: Percival Lowell was born, developed detailed maps of Mars and believed in an advanced Martian civilization.\n%D1992: US President George H. W. Bush issued National Space Policy Directive 6, \"Space Exploration Initiative,\" calling for human expeditions to Mars.",
				73 => "Equirria, a festival of horse racing dedicated to Mars. Established by Romulus himself in the early days of Rome. It is held in the Campus Martius in Roma, or the Campus Martialis if the former is flooded.\n%D1834: Giovanni Schiaparelli was born, developed first detailed maps of Mars and originated the system of areographic nomenclature in use today.\n%D1897: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the ninth of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				76 => "Agonalia, a festival sacred to Mars.",
				78 => "First day of Quinquatria, a celebration sacred to Mars, the first of five days. The Salii (priests of Mars) dance in the comitium (attended by the pontiffs and the symbolic representatives of the army--the tribuni celerum), and the sacred arma ancilia are purified. In this sense, it is a ritual preparation for the season`s coming military campaigns.",
				79 => "Second day of Quinquatria, a celebration sacred to Mars, the second of five days. The Salii (priests of Mars) dance in the comitium (attended by the pontiffs and the symbolic representatives of the army--the tribuni celerum), and the sacred arma ancilia are purified. In this sense, it is a ritual preparation for the season`s coming military campaigns.",
				80 => "Quinquatria, a celebration sacred to Mars, the third of five days. The Salii (priests of Mars) dance in the comitium (attended by the pontiffs and the symbolic representatives of the army--the tribuni celerum), and the sacred arma ancilia are purified. In this sense, it is a ritual preparation for the season`s coming military campaigns.",
				81 => "Third day of Quinquatria, a celebration sacred to Mars, the fourth of five days. The Salii (priests of Mars) dance in the comitium (attended by the pontiffs and the symbolic representatives of the army--the tribuni celerum), and the sacred arma ancilia are purified. In this sense, it is a ritual preparation for the season`s coming military campaigns.\n%D2010: Contact with Mars Exploration Rover A (Spirit) was lost.",
				82 => "Fourth day of Quinquatria, a celebration sacred to Mars, the last of five days. The Salii (priests of Mars) dance in the comitium (attended by the pontiffs and the symbolic representatives of the army--the tribuni celerum), and the sacred arma ancilia are purified. In this sense, it is a ritual preparation for the season`s coming military campaigns.\nTubilustrum, the lustration of trumpets, sacred to Mars. An ewe is sacrificed to sanctify the trumpets used in many of the public rites in preparation for both the coming sacral year and the military campaigning season. It is accompanied by a dance of the Salii.\n%D1912: Wernher von Braun was born, developed the Saturn V launch vehicle, author of \"The Mars Project.\"",
				83 => "%D1899: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 31st of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				84 => "%D1969: Mariner 7 was launched.",
				86 => "%D1969: Mars 1969A failed during launch.\n%D1989: Contact with Phobos 2 was lost after 58 sols in Mars orbit.",
				87 => "%D1938: Edgar Rice Burroughs began writing \"The Synthetic Men of Mars.\"",
				89 => "%D1899: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 32nd of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				91 => "%D1897: H. G. Wells` \"The War of the Worlds\" began serialization in England in \"Pearson`s\" (date approximate).\n%D1899: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 33rd of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%D1930: Edgar Rice Burroughs` \"A Fighting Man of Mars\" began serialization in \"Blue Book\" (date approximate).\n%D1952: Leigh Brackett`s \"The Last Days of Shandakor\" appeared in \"Startling Stories\" (date approximate).\n%D1964: Philip K. Dick`s \"Martian Time-Slip\" was published (date approximate).\n%D1966: Philip K. Dick`s \"We Can Remember It for You Wholesale\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate). It was the basis of of the 1990 motion picture \"Total Recall.\"\n%D2003: Approximate time setting for \"April 2003: The Musicians\" in Ray Bradbury`s \"The Martian Chronicles.\"\n%D2005: Approximate time setting for \"April 2005: Usher II\" in Ray Bradbury`s \"The Martian Chronicles.\"\n%D2026: Approximate time setting for \"April 2026: The Long Years\" in Ray Bradbury`s \"The Martian Chronicles.\"\n%D2000: Approximate time setting for \"April 2000: The Third Expedition\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				96 => "%D1952: Ray Bradbury`s \"The Wilderness\" was published in \"The Philadelphia Enquirer.\"\n%D1953: The motion picture \"Abbott and Costello Go to Mars\" was released.",
				98 => "%D1916: Edgar Rice Burroughs` \"Thuvia, Maid of Mars\" began serialization in \"All-Story Weekly.\"",
				101 => "%D1921: The motion picture \"A Message from Mars\" was released.",
				102 => "%D1980: Contact with Viking Lander 2 was lost after 1,280 sols in Utopia Planitia.\n%D2002: The motion picture \"Stranded\" was released.",
				104 => "%D1629: Christiaan Huygens was born, drew the first identifiable Martian surface feature.\n%D1969: Mars 1969B failed during launch.",
				105 => "%D1886: Henri Perrotin and Louis Thollon confirmed the existence of the \"canali.\"",
				109 => "%D2010: The First International Conference on Mars Sedimentology and Stratigraphy opened in El Paso, Texas.",
				110 => "%D1848: Kurd Lasswitz was born, author of \"Two Planets.\"\n%D1955: The motion picture \"Conquest of Space\" was released.\n%D1990: The motion picture \"Martians Go Home\" was released.",
				112 => "%D1953: The motion picture \"Invaders From Mars\" was released.\n%D2014: The Second Humans to Mars Summit opened in Washington, DC.",
				115 => "%D1997: \"The Workshop on Early Mars\" opened in Houston, Texas.",
				117 => "%D1955: The motion picture \"Devil Girl From Mars\" was released.",
				119 => "%D1981: \"The Case For Mars I\" conference opened in Boulder, Colorado.",
				121 => "%D1897: H. G. Wells` \"The War of the Worlds\" began serialization in the USA in \"Cosmopolitan\" (date approximate).\n%D1950: Ray Bradbury`s \"The Martian Chronicles\" was published (date approximate).\n%D1963: The television series \"My Favorite Martian\" ended with the 107th episode.\n%D1979: Robert F. Young`s \"The First Mars Mission\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).\n%D1993: NASA issued \"Mars Design Reference Mission 1.0\" (date approximate).\n%D1995: Kim Stanley Robinson`s \"Green Mars\" was published by Spectra.\n%D1998: A meteorite, designated Dar al Gani 476, was discovered near Dar al Gani, Libya. The meteorite was later identified as having originated on Mars.",
				123 => "%D1990: Voicing his opposition to the Bush Administration`s Space Exploration Initiative, Senator Albert Gore, Jr. told his fellow legislators, \"Before discussing a mission to Mars, the Administration needs a mission to reality.\"\n%D1991: \"America at the Threshold: America`s Space Exploration Initiative\" (\"The Stafford Report\") called for manned missions to Mars beginning in 2014.",
				125 => "%D2015: The Third Humans to Mars Summit opened in Washington, DC.",
				126 => "%D1915: Orson Welles was born, leader of the Mercury Theater of the Air, which performed the \"Panic Broadcast.\"\n%D2013: The First Humans to Mars Summit opened in Washington, DC.",
				127 => "%D1856: Angelo Secchi described \"a large, triangular patch.\" Then known as the Hourglass Sea, Secchi instead called it the \"Canale Atlantico,\" the first use of the word \"canale\" applied to Mars. The feature was later named Syrtis Major by Giovanni Schiaparelli.\n%D1998: A meteorite, designated Dar al Gani 876, was discovered near Dar al Gani, Libya. The meteorite was later identified as having originated on Mars.",
				128 => "%D1971: Mariner 8 failed during launch.",
				130 => "%D1971: Kosmos 419 was launched, but failed to leave Earth orbit.",
				131 => "%D1990: George H. W. Bush became the first US President to set a target date for sending humans to Mars: \"I believe that before America celebrates the 50th anniversary of its landing on the Moon, the American flag should be planted on Mars.\"",
				132 => "%D2001: The First Australian Mars Exploration Conference opened in Melbourne, Victoria.",
				133 => "%D1907: The Martians launched the first of ten cylinders toward Earth, beginning the War of the Worlds.\n%D2005: \"Star Trek: Enterprise\" episode \"Terra Prime\" aired.",
				134 => "%D1907: During the War of the Worlds, the Martians launched the second of ten cylinders toward Earth.",
				135 => "%D1907: During the War of the Worlds, the Martians launched the third of ten cylinders toward Earth.",
				136 => "%D1907: During the War of the Worlds, the Martians launched the fourth of ten cylinders toward Earth.\n%D1992: Sailor Mars first appeared in \"An Uncharmed Life\" episode of the animated television series \"Sailor Moon\" (Japan air date).",
				137 => "%D1907: During the War of the Worlds, the Martians launched the fifth of ten cylinders toward Earth.",
				138 => "%D1907: During the War of the Worlds, the Martians launched the sixth of ten cylinders toward Earth.\n%D1988: Soviet President Mikhail Gorbachev called for a joint unmanned Mars mission with the USA.",
				139 => "%D1907: During the War of the Worlds, the Martians launched the seventh of ten cylinders toward Earth.\n%D1971: Mars 2 was launched.",
				140 => "%D1907: During the War of the Worlds, the Martians launched the eighth of ten cylinders toward Earth.",
				141 => "%D1907: During the War of the Worlds, the Martians launched the nintn of ten cylinders toward Earth.\n%D2012: The Third International Conference on Early Mars opened in Lake Tahoe, Nevada.",
				142 => "Tubilustrum, the lustration of trumpets, sacred to Mars. An ewe is sacrificed to sanctify the trumpets used in many of the public rites. It is accompanied by a dance of the Salii.\n%D1907: During the War of the Worlds, the Martians launched the last of ten cylinders toward Earth.",
				143 => "%D1897: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 10th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%D1909: Nikola Tesla discussed communication with Mars in an article in \"The New York Times.\"",
				145 => "%D2008: Phoenix landed in Green Valley, Vastitas Borealis.",
				146 => "%D1988: The Planetary Society`s \"Mars Declaration\" was published in \"The Washington Post.\"\n%D1993: \"The Case For Mars V\" conference opened in Boulder, Colorado.",
				148 => "%D1971: Mars 3 was launched.",
				149 => "Ambarvalia, the ritual purification of the fields, connected with the agricultural deities Ceres, Bacchus, and Mars. It is the \"beating of the bounds\", when the boundaries between fields are purified by a procession of sacrificial animals, the suovetaurilia.",
				150 => "%D1971: Mariner 9 was launched.",
				151 => "%D1931: Edgar Rice Burroughs` \"A Fighting Man of Mars\" was published in \"Metropolitan.\"",
				152 => "This day is sacred to Mars. It is the anniversary of the dedication of the Temple of Mars near the Capena gate.\n%D1894: The Lowell Observatory began its study of Mars. Eighteen months later, Percival Lowell was published the observations of the 1894 opposition in a popular book, \"Mars.\"\n%D1941: Edgar Rice Burroughs` \"The Black Pirates of Barsoom\" appeared in \"Amazing Stories\" (date approximate). The story was later included in the anthology \"Llana of Gathol.\"\n%D1984: Lin Carter`s \"Down to a Sunless Sea\" was published by DAW (date approximate).\n%D1990: The motion picture \"Total Recall\" was released.\n%D2001: Approximate time setting for \"June 2001: And the Moon Be Still as Bright\" in Ray Bradbury`s \"The Martian Chronicles.\"\n%D2003: Approximate time setting for \"June 2003: Way in the Middle of the Air\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				153 => "%D1895: \"The San Francisco Chronicle\" reported that an observer had read the name of the Almighty in Hebrew letters on the surface of Mars.\n%D1950: The motion picture \"Rocketship X-M\" was released.\n%D1997: Kim Stanley Robinson`s \"Blue Mars\" was published by Spectra.\n%D2003: Mars Express was launched.",
				154 => "This day is sacred to Bellona.\n%D1963: The first \"Symposium on the Exploration of Mars\" opened in Denver, Colorado. This was the first conference devoted to the exploration of Mars via spacecraft.",
				155 => "%D1899: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the last of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%D1990: \"The Case For Mars IV\" conference opened in Boulder, Colorado.\n%D2005: The motion picture \"Crimson Force\" was released.",
				157 => "%D1986: The motion picture \"Invaders From Mars\" was released.",
				160 => "%D1930: Lin Carter was born, author of \"The Man Who Loved Mars.\"",
				161 => "%D2003: Mars Exploration Rover Spirit was launched.",
				164 => "Minor Quinquatria, a festival sacred to Minerva and Mars.",
				165 => "%D1907: The first Martian cylinder landed on Horsell Common, east of Horsell. The Deputation advanced upon the pit, waving a white flag. There was a flash of light, an invisible ray of heat flashed from man to man, and each burst into flame, killing about 40 people.",
				166 => "%D1907: During the War of the Worlds, the second Martian cylinder landed on the Addlestone Golf Links. A battalion of the Cardigan Regiment rushed the Horsell pit in skirmish order and was annihilated by the Heat-Ray. The fighting-machine then destroyed Woking.",
				167 => "%D1907: During the War of the Worlds, the third Martian cylinder landed north of Pyrford, completing the Surrey Triangle. Five Martian fighting-machines advanced down the Wey River to the confluence of the Thames. Royal Army batteries engage the Martians, destroying one fighting-machine, but Weybridge and Chertsey were destroyed by Heat-Ray. Later, the St. George's Hill battery damaged one fighting-machine, but was then destroyed. Seven Martian fighting-machines fanned out along a curved line between St. George's Hill, Weybridge, and the village of Send. The Martians discharged Black Smoke across the valley of the Thames, advancing through Street Cobham and Ditton. Richmond, Kingston, and Wimbledon were destroyed.",
				168 => "%D1907: During the War of the Worlds, the fourth Martian cylinder landed in Bushey Park, beginning the West End Triangle. The Martians advanced in a line from Hanwell in the north to Coombe and Malden in the south. Organized resistance by the British forces collapsed. The Martians went to and fro over the North Downs between Guildford and Maidstone, using the Black Smoke to eliminate any artillery batteries located there. Police organization in London broke down. The railway system collapsed.\n%D2000: A meteorite, designated Dhofar 378, was discovered near Dhofar, Oman. The meteorite was later identified as having originated on Mars.",
				169 => "%D1907: During the War of the Worlds, the fifth Martian cylinder landed in Sheen, and the sixth Martian cylinder landed in Wimbledon, completing the West End Triangle.",
				170 => "%D1907: During the War of the Worlds, the seventh Martian cylinder landed in Primrose Hill, where the invaders established their new headquarters. \"HMS Thunder Child\" made a suicide run at three fighting-machines the mouth of the Blackwater to cover the escape of passenger vessels. Two fighting-machines were destroyed. \"Thunder Child\" was also destroyed. \n%D1963: Mars 1 passed Mars three months after contact was lost.\n%D1976: Viking 1 entered Mars orbit.\n%D1995: The \"Mars Together\" conference opened in Palo Alto, California.",
				171 => "%D1907: During the War of the Worlds, the eighth Martian cylinder landed (unreported). A fighting-machine destroyed Leatherhead, with every soul in it.",
				172 => "%D1907: During the War of the Worlds, the ninth Martian cylinder landed (unreported). The Martians vacated the Sheen cylinder except for one fighting-machine and one handling-machine.",
				173 => "%D1907: During the War of the Worlds, the last of ten Martian cylinder landed (unreported).",
				176 => "%D1897: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 11th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				178 => "%D1897: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 12th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%D1941: Edgar Rice Burroughs` \"The Black Pirates of Barsoom\" was published in \"Amazing Stories.\"\n%D2001: A meteorite, designated Sayh al Uhaymir 060, was discovered near Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.",
				179 => "%D1911: An unfortunate dog in Nakhla, Egypt was struck by part of a meteorite and killed. In the 1980s, the Nakhla meteorite was identified as having originated on Mars.",
				180 => "%D2005: The motion picture \"The War of the Worlds\" was released.",
				182 => "%D1911: Writing down his daydreams on the backs of old letterheads of previously failed businesses, Edgar Rice Burroughs used free time at his office to begin \"A Princess of Mars\" (date approximate).\n%D1960: Arthur C. Clarke`s \"Crime on Mars,\" later titled \"The Trouble with Time,\" was published in \"Ellery Queen\" (date approximate).\n%D1976: Lin Carter`s \"The Martian El Dorado of Parker Wintley\" was published in \"The DAW Science Fiction Reader\" (date approximate).\n%D1992: Michael Cassutt`s \"The Last Mars Trip\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).\n%D1997: William K. Hartmann`s \"Mars Underground\" was published (date approximate).\n%D1997: NASA issued \"Mars Design Reference Mission 3.0\" (date approximate).\n%D2009: NASA issued \"Human Exploration of Mars Design Reference Architecture 5.0\" (date approximate).",
				185 => "%D1907: The last Martians succumbed to terrestrial disease, ending the War of the Worlds.\n%D1997: Mars Pathfinder landed in Ares Vallis. The spacecraft was later renamed the Carl Sagan Memorial Station.\n%D2008: The Eighth Australian Mars Exploration Conference opened in Adelaide, South Australia.",
				186 => "%D1867: Andrew Ellicott Douglass was born; with William Henry Pickering, he devised the first known Martian calendar to be used in scientific research.",
				187 => "%D1997: Sojourner, the first roving vehicle on Mars, deployed from the Carl Sagan Memorial Station.",
				188 => "%D1907: Robert A. Heinlein was born, author of \"Red Planet,\" \"Podkayne of Mars,\" and \"Stranger in a Strange Land.\"\n%D1988: Phobos 1 was launched.",
				189 => "%D2003: Mars Exploration Rover Opportunity was launched.",
				190 => "%D2007: The Seventh International Conference on Mars opened in Pasadena, California.\n%D2010: The Tenth Australian Mars Exploration Conference opened in Melbourne, Victoria.",
				191 => "%D1984: \"The Case For Mars II\" conference opened in Boulder, Colorado.",
				193 => "%D1988: Phobos 2 was launched.\n%D2002: The Second Australian Mars Exploration Conference opened in Sydney, New South Wales.",
				194 => "%D2007: The Seventh Australian Mars Exploration Conference opened in Perth, Western Australia.",
				195 => "%D2014: The Eighth International Conference on Mars opened in Pasadena, California.",
				196 => "%D1965: Mariner 4 achieved the first flyby of Mars, transmitted 22 images of a cratered surface.",
				198 => "%D1964: The motion picture \"Robinson Crusoe on Mars\" was released.\n%D1996: \"The Case for Mars VI\" conference opened in Boulder, Colorado.\n%D2009: The Ninth Australian Mars Exploration Conference opened in Adelaide, South Australia.",
				199 => "%D1958: A few months before it was transformed into a space agency by the National Aeronautics and Space Act, a report by the National Advisory Committee on Aeronautics projected that a manned Mars mission might be undertaken in 1977.\n%D1987: \"The Case For Mars III\" conference opened in Boulder, Colorado.\n%D1999: The Fifth International Conference on Mars opened in Pasadena, California.\n%D2011: A meteorite, designated Tissint, was observed to fall in Tata Province in the Guelmim-Es Semara region of Morocco. The meteorite was later identified as having originated on Mars.",
				201 => "%D1976: Viking Lander 1 achieved first successful landing on Mars in Chryse Planitia. The spacecraft was later renamed the Thomas A. Mutch Memorial Station.\n%D1989: US President H. W. George Bush launched the Space Exploration Initiative from the steps of the National Air & Space Museum on the 20th anniversary of the Apollo 11 moon landing, calling for \"a journey into tomorrow, a journey to another planet - a manned mission to Mars.\"\n%D2003: The Sixth International Conference on Mars opened in Pasadena, California.",
				202 => "%D1973: Mars 4 was launched.\n%D1986: The \"NASA Mars Conference\" opened in Washington, DC.",
				203 => "%D2011: The Eleventh Australian Mars Exploration Conference opened in Perth, Western Australia.",
				204 => "%D1914: Edgar Rice Burroughs began writing \"The Gods of Mars.\"",
				205 => "%D1940: Edgar Rice Burroughs began writing \"John Carter and the Pits of Horz,\" first of a series for new book. The story was later was published under the title \"The City of Mummies.\"\n%D1948: Marvin the Martian debuted in animated short film \"Haredevil Hare.\"",
				206 => "%D1973: Mars 5 was launched.\n%D1976: Viking Orbiter 1 frame 35A72 imaged Cydonia Face and Pyramids.\n%D1978: Contact with Viking Orbiter 2 was lost after 697 sols in Mars orbit.",
				208 => "%D1781: William Herschel calculated a rotational period for Mars of 24 hours, 39 minutes, and 21.67 seconds. He also confirmed that the north polar spot was eccentric to the pole.",
				209 => "%D1894: Nice Observatory astronomer Stephane Javelle observed a luminous projection in the region of the southern terminator of Mars.",
				211 => "%D1894: Henri Joseph Anastase Perrotin, director of the Nice Observatory, sent the following telegram: \"Projection lumineuse dans région australe du terminateur de Mars observeé par Javelle 28 Juillet 16 heures Perrotin.\"\n%D2009: \"The Twelfth International Mars Society Convention\" opened in College Park, Maryland.",
				212 => "%D1969: Mariner 6 passed Mars, returned 26 near encounter images.\n%D2003: The Fourth Australian Mars Exploration Conference opened in Adelaide, South Australia.",
				213 => "%D1941: Edgar Rice Burroughs` \"The Yellow Men of Mars\" appeared in :Amazing Stories: (date approximate). The story was later included in the anthology \"Llana of Gathol.\"\n%D1963: Philip K. Dick`s \"All We Marsmen\" began serialization in \"Worlds of Tomorrow\" (date approximate).\n%D1968: NASA terminated the contract to procure additional Saturn Vs, ending production with Saturn 515, and abandoning the heavy-lift launch capability required to launch piloted Mars missions.\n%D1999: Approximate time setting for \"August 1999: The Summer Night\" in Ray Bradbury`s \"The Martian Chronicles.\"\n%D1999: Approximate time setting for \"August 1999: The Earth Men\" in Ray Bradbury`s \"The Martian Chronicles.\"\n%D2000: A meteorite, designated Sayh al Uhaymir 051, was discovered near Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.\n%D2001: Approximate time setting for \"August 2001: The Settlers\" in Ray Bradbury`s \"The Martian Chronicles.\"\n%D2002: Approximate time setting for \"August 2002: Night Meeting\" in Ray Bradbury`s \"The Martian Chronicles.\"\n%D2005: Approximate time setting for \"August 2005: The Old Ones\" in Ray Bradbury`s \"The Martian Chronicles.\"\n%D2026: Approximate time setting for \"August 2026: There Will Come Soft Rains\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				214 => "%D1894: The British scientific journal \"Nature\" reported the observation of Stephane Javelle.\"If we assume the light to be on the planet itself, then it must either have a physical or human origin; so it is expected that the old idea that the Martians are signalling to us will be revived.\"H. G. Wells' narrator later speculated, \"I am inclined to think that this blaze may have been the casting of the huge gun, in the vast pit sunk into their planet, from which their shots were fired at us.\"",
				215 => "%D1945: The motion picture \"The Purple Monster Strikes\" was released.\n%D1967: NASA Manned Spacecraft Center issued \"Request for Proposal No. BG721-28-7-528P, Planetary Surface Sample Return Probe Study for Manned Mars/Venus Reconnaissance/Retrieval Missions\" in the 1975-1982 time frame. Rep. Joseph Karth, acting chairman of the House Subcommittee on NASA Oversight, who had been fighting an uphill battle to preserve Project Voyager funding, later expressed his exasperation, for the move cast the program of Saturn V-launched unmanned orbiters and landers in the role of a foot in the door for manned follow-on missions to the planets: \"Very bluntly, a manned mission to Mars or Venus by 1975 or 1977 is now and always has been out of the question, and anyone who persists in this kind of misallocation of resources at this time will be stopped.\"\n%D2006: \"The Ninth International Mars Society Convention\" opened in Washington, DC.\n%D2012: \"The Fifteenth International Mars Society Convention\" opened in Pasadena California.",
				216 => "%D1976: Viking Orbiter 1 frames 43A01 through 43A04 imaged Deuteronilus Crater Pyramid.\n%D2007: Phoenix lander was launched.\n%D2011: \"The Fourteenth International Mars Society Convention\" opened in Dallas, Texas.",
				217 => "%D1969: Mariner 7 passed Mars, returned 33 near encounter images.\n%D1973: Mars 6 was launched.\n\"%D2010: The Thirteenth International Mars Society Convention\" opened in Dayton, Ohio.",
				218 => "%D1965: Zond 2 passed Mars four months after contact was lost.\n%D1996: The National Science Foundation announced evidence for possible early life on Mars in Antarctic meteorite ALH84001.\n%D2001: \"The Infinite Worlds of H.G. Wells\" episode \"The Crystal Egg\" aired.\n%D2012: Mars Science Laboratory \"Curiosity\" landed in Aeolis Palus (\"Bradbury Landing\") in Gale Crater.",
				219 => "%D1976: Viking 2 entered Mars orbit.\n%D1980: Contact with Viking Orbiter 1 was lost after 1,469 sols in Mars orbit.\n%D2014: \"The Seventeenth International Mars Society Convention\" opened in League City, Texas.",
				220 => "%D2002: \"The Fifth International Mars Society Convention\" opened in Boulder, Colorado.",
				221 => "%D1965: \"I say let`s do it quickly and establish a foothold on a new planet while we still have one left to take off from.\" --Wernher von Braun, \"Manned Mars Landing,\" \"Aviation Week & Space Technology.\"\n%D1973: Mars 7 was launched.",
				222 => "%D1877: Asaph Hall gave up a search for Martian moons. The following night, having resumed his search at the insistence of his wife, Angelina, he detected a faint object near Mars, which he later named Deimos.\n%D2000: \"The Third International Mars Society Convention\" opened in Toronto, Ontario.",
				223 => "%D1989: \"Mars is essentially in the same orbit. Mars is somewhat the same distance from the sun, which is very important. We have seen pictures where there are canals, we believe, and water. If there is water, that means there is oxygen. If oxygen, that means we can breathe.\" -- US Vice President Dan Quayle.\n\"%D2005: The Eighth International Mars Society Convention\" opened in Boulder, Colorado.",
				224 => "%D1877: \"The New York Times\" asked, \"Is Mars inhabited?\" in an editorial. As the best opposition since 1798 approached, questions in the popular mind came to the fore and the possibility of life on Mars was discussed in the press.\n%D1999: \"The Second International Mars Society Convention\" opened in Boulder, Colorado.\n%D2005: Mars Reconnaissance Orbiter was launched.",
				225 => "%D1998: \"The Founding Convention of the Mars Society\" opened in Boulder, Colorado.",
				226 => "%D2003: \"The Sixth International Mars Society Convention\" opened in Eugene, Oregon.\n\"%D2008: The Eleventh International Mars Society Convention\" opened in Boulder, Colorado.",
				227 => "%D1998: \"The Founding Declaration of the Mars Society\" was adopted unanimously in Boulder, Colorado.\n%D2013: \"The Sixteenth International Mars Society Convention\" opened in Boulder, Colorado.",
				228 => "%D1967: U.S. Congress deleted funding for Voyager, a program of Saturn V-launched unmanned orbiters and landers, then scheduled for its first missions in 1973. The program was later down-scoped and renamed \"Viking.\"",
				229 => "%D1877: Asaph Hall discovered Phobos.",
				230 => "%D1845: Omsby MacKnight Mitchel observed a white patch detached from the south polar cap of Mars. The feature became known as the Mountains of Mitchel. Actually it is a depression.\n%D1877: Asaph Hall announced the discovery of Mars` two moons. At the suggestion of Henry Madan, the Science Master of Eton, England, Hall named the moons Phobos and Deimos.",
				231 => "%D2005: The Fifth Australian Mars Exploration Conference opened in Canberra, ACT.",
				232 => "%D1956: A dust storm began with a bright cloud over the Hellas-Noachis region that spread to engulf the whole planet by mid-September.\n%D1975: Viking 1 was launched.\n%D2004: \"The Seventh International Mars Society Convention\" opened in Chicago, Illinois.",
				233 => "%D1898: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 22nd of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%D1907: I. M. Levitt was born, inventor of a Martian calendar and the first Earth-Mars mechanical date-time computer.\n%D1999: A meteorite, designated Dar al Gani 975, was discovered near Dar al Gani, Libya. The meteorite was later identified as having originated on Mars.\n%D2000: The Second International Conference on Mars Polar Science and Exploration opened in Reykjavik, Iceland.",
				234 => "%D1897: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 13th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%D1920: Ray Bradbury was born, author of \"The Martian Chronicles.\"\n%D1993: Contact with Mars Observer was lost enroute to Mars.\n%D2003: The Third Australian Mars Exploration Conference opened in Perth, Western Australia.",
				235 => "%D2001: \"The Fourth International Mars Society Convention\" opened at Stanford University, California.",
				236 => "%D1638: Two years after making his first drawing of Mars, Neapolitan lawyer and amateur astronomer Francesco Fontana produced a second drawing, which like the first, featured a dark spot, which has been attributed to an optical defect in his telescope.",
				237 => "%D1865: A meteorite was observed to fall near Shergotty, India. In the 1980s, the Shergotty meteorite was identified as having originated on Mars.\n%D1993: Mars Observer passed Mars three days after contact was lost.",
				238 => "%D1953: The motion picture \"The War of the Worlds\" was released.\n%D2003: In perihelic opposition, Mars made its closest approach to Earth in 60,000 years, coming within 55.8 million kilometers.",
				239 => "%D1911: \"Martians Build Two Immense Canals in Two Years\" reported in \"The New York Times.\"",
				240 => "%D1877: Henry Draper of New York and Edward Singleton Holden of the U.S. Naval Observatory in Washington claimed to have jointly discovered a third moon of Mars at Draper`s private observatory at Hastings-on-the-Hudson. This discovery proved to be false; in fact, the proposed moon did not even obey Kepler`s laws.",
				242 => "%D1976: Viking Orbiter 1 frame 70A13 imaged the Cydonia Face and Pyramids.",
				243 => "%D1981: The Thied International Colloquium on Mars opened in Pasadena, California.",
				244 => "%D1875: Edgar Rice Burroughs was born, author of the \"Barsoom\" series.\n%D1969: A NASA report to US President Richard Nixon`s Space Task Group, which was chartered to explore options for space efforts beyond the Apollo program, stated, \"Manned expeditions to Mars could begin as early as 1981.\"\n%D1993: Sailor Mars first appeared in the manga \"Run Run.\"\n%D2005: Approximate time setting for \"September 2005: The Martian\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				245 => "%D1988: Contact with Phobos 1 was lost en route to Mars.\n%D2007: \"The Tenth International Mars Society Convention\" opened in Los Angeles, California.",
				246 => "%D1976: Viking Lander 2 landed in Utopia Planitia.",
				247 => "%D1898: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 23rd of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				248 => "%D2014: The Fourteenth European Mars Convention opened in Podzamcze, Poland.",
				252 => "%D1975: Viking 2 was launched.",
				254 => "%D2002: Alex Irvine`s \"Pictures from an Expedition\" was published in \"In The Magazine of Fantasy and Science Fiction.\"",
				255 => "%D1877: Giovanni Schiaparelli began a detailed study of Mars for the purpose of drawing a new and accurate map of the planet.\n%D1897: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 14th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%D1997: Mars Global Surveyor entered Mars orbit.\n%D2011: The Third International Conference on Mars Polar Science and Exploration opened in Fairbanks, Alaska.",
				257 => "%D2012: The Twelfth Australian Mars Exploration Conference opened in Canberra, ACT.",
				258 => "%D1877: Giovanni Schiaparelli observed the canale Cyclops.\n%D1969: The report by US President Richard Nixon`s Space Task Group, \"The Post-Apollo Space Program: Directions for the Future,\" stated, \"We conclude that NASA has the demonstrated organizational competence and technology base, by virtue of the Apollo success and other achievements, to carry out a successful program to land man on Mars within 15 years.\" Nevertheless, the report backed away from an early manned landing on Mars, recommending that the focus for the next decades in space should be on the development of hardware and systems that would ultimately support a manned mission to Mars at the close of the 20th century.",
				261 => "%D1976: Viking Orbiter 1 frame 86A10 imaged the Utopia Faces.",
				262 => "%D1995: Sailor Mars first appeared in \"An Uncharmed Life\" episode of the animated television series \"Sailor Moon\" (USA air date).\n%D1996: US President Bill Clinton abandoned George Bush`s goal of a manned landing on Mars. His National Science and Technology Council`s statement of National Space Policy only mentioned \"a sustained program to support a robotic presence on the surface of Mars by year 2000 for the purposes of scientific research, exploration and technology development.\"\n%D2013: The motion picture \"The Last Days on Mars\" was released.",
				263 => "%D1896: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the second of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				264 => "%D1866: H. G. Wells was born, author of \"The War of the Worlds.\"",
				265 => "%D1877: Giovanni Schiaparelli observed the canale Ambrosia.\n%D2014: Mars Atmosphere and Volatile EvolutioN Mission (MAVEN) entered Mars orbit.",
				266 => "%D1999: Two meteorites, designated Los Angeles 001 and 002, were discovered in the Mojave Desert near Los Angeles, California. The meteorites were later identified as having originated on Mars.",
				267 => "%D2014: Mars Orbiter Mission (MOM), also called Mangalyaan, entered Mars orbit.",
				268 => "%D1924: The motion picture \"Aelita: Queen of Mars\" was released.\n%D1992: Mars Observer was launched.",
				269 => "%D2003: The Third European Mars Convention opened in Bremen, Germany.",
				270 => "%D1919: Edgar Rice Burroughs` \"The Warlord of Mars\" was published by McClurg.\n%D1940: Edgar Rice Burroughs began writing \"The Black Pirates of Barsoom,\" part 2 of a new Mars series.\n%D2002: The Second European Mars Convention opened in Rotterdam, Netherlands.",
				271 => "%D2001: The First European Mars Convention opened in Paris, France.",
				272 => "%D1963: The television series \"My Favorite Martian\" debuted.",
				273 => "%D2011: The Eleventh European Mars Convention opened in Neuchatel, Switzerland.\n%D2013: The Thirteeth Australian Mars Exploration Conference opened in Sydney, New South Wales.",
				274 => "%D1783: William Herschel observed the south polar cap of Mars. \"I am inclined to think that the white spot has some little revolution.... It is rather probable that the real pole, though within the spot, may lie near the circumference of it, or one-third of its diameter from one of the sides. A few days more will show it, as I shall now fix my particular attention on it.\"\n%D1895: \"The observations of 1894 have made it practically certain that the so-called `canals` of Mars are real, whatever may be their explanation,\" Princeton astronomer Charles A. Young declared in his article for \"Cosmopolitan,\" \"Mr. Lowell`s Theory of Mars\" (date approximate).\n%D1954: Alfred Coppel`s \"Mars Is Ours\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).\n%D1964: Leigh Brackett`s \"Purple Priestess of the Mad Moon\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).\n%D1977: Lin Carter`s \"The City Outside the World\" was published by Berkeley (date approximate).\n%D1993: Kim Stanley Robinson`s \"Red Mars\" was published by Spectra.\n%D2002: Approximate time setting for \"October 2002: The Shore\" in Ray Bradbury`s \"The Martian Chronicles.\"\n%D2026: Approximate time setting for \"October 2026: The Million-Year Picnic\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				275 => "%D2006: The Fourth International Conference on Mars Polar Science and Exploration opened in Davos, Switzerland.",
				276 => "%D1815: A meteorite was observed to fall near Chassigny, France. In the 1980s, the Chassigny meteorite was identified as having originated on Mars. It was the first Martian meteorite to be discovered.\n%D1898: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 24th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%D1913: Edgar Rice Burroughs drew a map of Barsoom at the request of his publisher.\n%D1962: A meteorite was observed to fall near Zagami, Nigeria. In the 1980s, this meteorite was identified as having originated on Mars.\n%D2000: Kim Stanley Robinson`s \"The Martians\" was published by Spectra.",
				277 => "%D1877: Giovanni Schiaparelli observed the canali Ganges and Phison.",
				280 => "%D1997: Contact with the Carl Sagan Memorial Station (Mars Pathfinder) was lost after 93 sols in Ares Vallis.",
				281 => "%D1976: \"Today we have touched Mars. There is life on Mars and it is us--extensions of our eyes in all directions, extensions of our mind, extensions of our heart and soul have touched Mars today. That`s the message to look for there: We are on Mars. We are the Martians!\" --Ray Bradbury, speaking at \"The Search for Life in the Solar System\" symposium in Pasadena, California.\n%D2002: A meteorite, designated Sayh al Uhaymir 150, was discovered near Dar al Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.\n%D2008: The motion picture \"Interplanetary\" was released.",
				282 => "%D2004: The Fourth European Mars Convention opened in Milton Keynes, England.",
				283 => "%D1877: Giovanni Schiaparelli observed Mare Erythraeum and Noachis to be obscured by clouds.\n%D1897: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 15th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%D1917: Edgar Rice Burroughs` \"A Princess of Mars\" was published by McClurg.\n%D1960: Mars 1960A, the first known Mars mission, failed during launch.",
				284 => "%D1997: The motion picture \"RocketMan\" was released.\n%D2010: US President Obama signed the NASA Authorization Act of 2010, which authorized a three-year funding commitment for continued development of the Orion spacecraft and the development of a new heavy-lift Space Launch System to support eventual manned lunar and interplanetary missions; however, the pace of these programs was slowed considerably in comparison to the canceled Constellation program.",
				285 => "%D1951: \"Tales of Tomorrow\" episode \"The Crystal Egg\" aired.\n%D2012: The Twelfth European Mars Convention opened in Munich, Germany.",
				286 => "%D2003: The Third International Conference on Mars Polar Science and Exploration opened in Lake Louise, Alberta.\n%D2006: The Sixth Australian Mars Exploration Conference opened in Melbourne, Victoria.",
				287 => "%D1898: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 25th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%D1960: Mars 1960B failed during launch.",
				288 => "Equus October, a cavalry exercise for the Roman army, a festival sacred to Mars.\n%D1829: Asaph Hall was born, discoverer of Phobos and Deimos.\n%D2009: The Ninth European Mars Convention opened in Bergamo, Italy.",
				290 => "%D2008: The Eighth European Mars Convention opened in Antwerp, Belgium.",
				291 => "%D1896: Charles A. Young of Princeton University discussed the question \"Is Mars Inhabited?\" in \"The Boston Herald.\"\n%D1998: The First International Conference on Mars Polar Science and Exploration opened in Houston, Texas.",
				292 => "Armilustrum, the lustration of shields, a festival sacred to Mars.\n%D2007: The Seventh European Mars Convention opened in Delft, Netherlands.",
				293 => "%D1877: Giovanni Schiaparelli observed the canale Eunostos.\n%D2006: The Sixth European Mars Convention opened in Paris, France.",
				295 => "%D1898: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 26th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%D2010: The Tenth European Mars Convention opened in Warsaw, Poland.",
				296 => "%D2001: Mars Odyssey entered Mars orbit.",
				297 => "%D1897: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 16th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%D1940: Edgar Rice Burroughs began writing \"Escape on Mars,\" part 3 of a new Mars series. The story was later was published under the title \"The Yellow Men of Mars.\"\n%D1962: Mars 1962A (Sputnik 29) was launched, but failed to leave Earth orbit.",
				298 => "%D1941: Edgar Rice Burroughs began writing \"The Skeleton Men of Jupiter,\" the first of a planned new John Carter series.\n%D2013: The Thirteenth European Mars Convention opened in Paris, France.",
				300 => "%D1879: Discussing the canali in a letter to Nathaniel Green, Giovanni Schiaparelli declared, \"It is [as] impossible to doubt their existence as that of the Rhine on the surface of the Earth.\"\n%D1898: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 27th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%D1972: Contact with Mariner 9 was lost after 338 sols in Mars orbit.\n%D2006: The motion picture \"Fascisti su Marte\" was released.",
				301 => "%D2008: Contact with Phoenix lander was lost.",
				303 => "%D1938: \"The Mercury Theater of the Air,\" starring Orson Welles, performed Howard Koch`s radio play \"Invasion from Mars.\" The broadcast terrorized the eastern USA.",
				305 => "%D1934: Edgar Rice Burroughs` \"The Swords of Mars\" began serialization in \"Blue Book\" (date approximate).\n%D1952: Isaac Asimov`s \"The Martian Way\" appeared in \"Galaxy Science Fiction\" (date approximate).\n%D1955: Fredric Brown`s \"Martians, Go Home\" was published (date approximate).\n%D1957: Isaac Asimov`s \"I`m in Marsport without Hilda\" appeared in \"Venture Science Fiction\" (date approximate).\n%D1962: Mars 1 was launched. Robert A. Heinlein`s \"Podkayne of Mars\" began serialization in \"If\" (date approximate).\n%D1963: Roger Zelazny`s \"A Rose for Ecclesiastes\" was published in \"The Magazine of Fantasy and Science Fiction\" (date approximate).\n%D1964: \"The Outer Limits\" episode \"The Invisible Enemy\" aired.%D2005: Approximate time setting for \"November 2005: The Luggage Store\" in Ray Bradbury`s \"The Martian Chronicles.\"\n%D2005: Approximate time setting for \"November 2005: The Off Season\" in Ray Bradbury`s \"The Martian Chronicles.\"\n%D2005: Approximate time setting for \"November 2005: The Watchers\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				306 => "%D1898: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 28th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.",
				308 => "%D1962: Mars 1962B (Sputnik 31) was launched, but failed to leave Earth orbit.\n%D2005: The Fifth European Mars Convention opened in Swindon, England.",
				309 => "%D1964: Mariner 3 failed during launch.\n%D2013: Mars Orbiter Mission (MOM), also called Mangalyaan, was launched.",
				311 => "%D1938: The motion picture \"Mars Attacks the World\" was released.\n%D1996: Mars Global Surveyor was launched.",
				313 => "%D1934: Carl Sagan was born, first president of the Planetary Society.",
				314 => "%D1879: Giovanni Schiaparelli discovered a small white patch in Tharsis and named it Nix Olympica. Nearly a century later, Mariner 9 imagery revealed this feature to be the largest mountain in the Solar System, and the feature was renamed Olympus Mons.\n%D1911: Percival Lowell reported \"Frost on Mars\" to \"The New York Times.\"\n%D2000: The motion picture \"Red Planet\" was released.",
				315 => "%D1875: Earl C. Slipher was born, observed Mars extensively.\n%D1951: The motion picture \"Flight to Mars\" was released.\n%D1964: The motion picture \"Pajama Party\" (also titled \"The Maid and the Martian\") was released.",
				317 => "%D1982: Contact with the Thomas A. Mutch Memorial Station (Viking Lander 1) was lost after 2,244 sols in Chryse Planitia.",
				318 => "%D1971: Mariner 9 became the first spacecraft to orbit Mars.",
				319 => "%D1996: The motion picture \"Space Jam\" starring Marvin the Martian was released.",
				320 => "%D1996: Mars 96 failed during launch.",
				321 => "%D1996: The third annual \"Lunar and Mars Exploration\" conference was held in San Diego, California.\n%D2002: A meteorite, designated Sayh al Uhaymir 120, was discovered near Dar al Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.",
				322 => "%D1940: Edgar Rice Burroughs began writing \"The Invisible Men of Mars,\" part 4 of a new Mars series.\n%D2013: Mars Atmosphere and Volatile EvolutioN Mission (MAVEN) was launched.",
				323 => "%D2003: A meteorite, designated Sayh al Uhaymir 125, was discovered near Dar al Sayh al Uhaymir, Oman. The meteorite was later identified as having originated on Mars.",
				324 => "%D1989: NASA`s \"Report of the 90-Day Study on Human Exploration of the Moon and Mars\" estimated the cost of a manned Mars program at $450 billion. Political support for manned Mars missions subsequently collapsed.",
				327 => "%D1930: The motion picture \"Just Imagine\" was released.\n%D1959: The motion picture \"The Angry Red Planet\" was released.",
				330 => "%D1999: Two meteorites, designated Sayh al Uhaymir 005 and 008, were discovered near Sayh al Uhaymir, Oman. The meteorites were later identified as having originated on Mars.\n%D2011: Mars Science Laboratory \"Curiosity\" was launched.",
				331 => "%D1971: Mariner 9 returned the first image of Deimos. Mars 2 Orbiter entered Mars orbit. Mars 2 Lander impacted on Mars.",
				332 => "%D1659: Christiaan Huygens drew the first sketch of Mars, including a dark, triangular area later named Syrtis Major, the first Martian surface feature identified from Earth. Huygens used his own design of telescope, which was of much higher quality than that of his predecessors and allowed a magnification of 50 times.\n%D1897: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 17th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%D1964: Mariner 4 was launched.\n%D1971: The First International Colloquium on Mars opened in Pasadena, California.",
				333 => "%D1896: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the third of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%D1922: Edgar Rice Burroughs` \"The Chessmen of Mars\" was published by McClurg.\n%D2000: A meteorite, designated Y000593, was discovered in the Yamato Mountains, Antarctica. The meteorite was later identified as having originated on Mars.",
				334 => "%D1964: Zond 2 was launched.\n%D1971: Mariner 9 returned the first image of Phobos.",
				335 => "%D1659: Christiaan Huygens noted, \"The rotation of Mars, like that of the Earth, seems to have a period of 24 hours.\"\n%D1895: Percival Lowell`s \"Mars\" was published (date approximate).\n%D1913: Edgar Rice Burroughs` \"The Warlord of Mars\" began serialization in \"All-Story\" (date approximate).\n%D1974: Lin Carter`s \"The Valley Where Time Stood Still\" was published by DAW (date approximate).\n%D2001: Approximate time setting for \"December 2001: The Green Morning\" in Ray Bradbury`s \"The Martian Chronicles.\"\n%D2005: Approximate time setting for \"December 2005: The Silent Towns\" in Ray Bradbury`s \"The Martian Chronicles.\"",
				336 => "%D1971: Mars 3 Orbiter entered Mars orbit. Contact with Mars 2 Lander was lost shortly after landing. No useful data was returned.",
				337 => "%D1999: Contact with Mars Polar Lander was lost prior to landing on Mars.\n%D2000: A meteorite, designated Y000749, was discovered in the Yamato Mountains, Antarctica. The meteorite was later identified as having originated on Mars.",
				338 => "%D1896: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the fourth of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%D1996: Mars Pathfinder was launched.\n%D1998: A meteorite, designated Yamato 980459, was discovered in the Yamato Mountains, Antarctica. The meteorite was later identified as having originated on Mars. The Outer Limits episode \"Phobos Rising\" aired.\n%D2014: The Orion spacecraft, designed for manned interplanetary missions, was tested successfully in Earth orbit on an unmanned flight.",
				339 => "%D1898: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the 29th of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.\n%D1961: NASA`s Office of Manned Space Flight published its \"Long Range Plan,\" which projected the launch of a manned Mars flyby mission in 1970 and a manned Mars landing mission in 1975.",
				341 => "%D1915: Leigh Brackett was born, author of the Eric John Stark series of Martian stories.\n%D1988: Soviet President Mikhail Gorbachev called for a joint manned Mars mission with the USA.",
				345 => "%D1998: Mars Climate Orbiter was launched.",
				347 => "%D1896: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the fifth of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.n\n%D1996: The motion picture \"Mars Attacks!\" was released.",
				349 => "%D1896: As documented by Thomas Flournoy in his classic study in psychology \"From India to the Planet Mars: A Study of a Case of Somnambulism,\" the spiritualist medium Catherine Elise Mueller (a.k.a. Helene Smith) engaged in the sixth of a series of 34 seances, in which she communicated with Martians, spoke in their language, and wrote in Martian script.n\n%D2003: A meteorite, designated MIL 03346, was discovered in the Miller Range of the Transantarctic Mountains. The meteorite was later identified as having originated on Mars.",
				350 => "%D1917: Arthur C. Clarke was born, author of \"The Sands of Mars.\"\n%D1928: Philip K. Dick was born, author of \"Martian Time-Slip.\"\n%D1994: A meteorite, designated QUE 94201, was discovered in the Queen Alexandra Range, Antarctica. The meteorite was later identified as having originated on Mars.",
				356 => "%D1958: The motion picture \"Quatermass and the Pit\" was released.\n%D1988: A meteorite, designated LEW 88516, was discovered at Lewis Cliff, Antarctica. The meteorite was later identified as having originated on Mars.",
				358 => "%D1644: Two patches on the lower part of the disk of Mars were described by a Neapolitan Jesuit named Father Bartoli.",
				359 => "%D2003: Mars Express entered orbit. Contact with Beagle 2 was lost prior to landing in Isidis Planitia.",
				360 => "%D1957: The motion picture \"Mars and Beyond\" was released.",
				361 => "%D1984: A meteorite, designated ALH 84001, was discovered in the Allan Hills, Antarctica. In 1993, the meteorite was identified as having originated on Mars. In 1996, a team of scientists announced evidence on fossilized Martian life in the meteorite.",
				363 => "%D1930: The motion picture \"Mars\" was released.\n%D1977: A meteorite, designated ALHA 77005, was discovered in the Allan Hills, Antarctica. In the 1980s, the meteorite was identified as having originated on Mars.\n%D2009: The motion picture \"Princess of Mars\" was released.",
				364 => "%D1610: Galileo Galilei discovered that, like the Moon, Mars had a phase.",
				365 => "%D1864: Robert G. Aitken was born, developed the first complete Martian calendar.",
				_ => string.Empty
			}).Replace("%D", "On this day in ");

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
				FormatWrite("Warning: The Gregorian calendar did not exist in the year specified.\n");
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
			bool _CLOCK;
			bool dateTemp;
			string? input;

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

				string[]? n;
				DateTime dtCapture;

				switch (input?.Trim())
				{
					case "":
						dtCapture = DateTime.UtcNow;

						dateTemp = earthDate;
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

						SolNomen = uint.Parse(Console.ReadKey().KeyChar.ToString());
						if (SolNomen < 0 || SolNomen > 2)
						{
							SolNomen = 1;
							FormatWrite("Invalid selection.");
						}
						else
							Console.Clear();

						break;
					case "r":
						dateTemp = earthDate;
						earthDate = true;
						_CLOCK = true;

						while (_CLOCK)
						{
							dtCapture = DateTime.UtcNow;

							while (DateTime.UtcNow.Subtract(dtCapture).Milliseconds < 100)
								if (Console.KeyAvailable)
									_CLOCK = false;

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
						n = input?.Split(' ');

						if (n?.Length == 6)
						{
							try
							{
								if (earthDate)
								{
									eYear = int.Parse(n[5]);
									eMonth = int.Parse(n[4]);
									eDay = int.Parse(n[3]);
									eHour = int.Parse(n[0]);
									eMin = int.Parse(n[1]);
									eSec = (int)Math.Floor(double.Parse(n[2]));
									eMil = (int)(double.Parse(n[2]) % 1 * 1000);
								}
								else
								{
									mYear = int.Parse(n[5]);
									mMonth = int.Parse(n[4]);
									mDay = int.Parse(n[3]);
									mHour = int.Parse(n[0]);
									mMin = int.Parse(n[1]);
									mSec = (int)Math.Floor(double.Parse(n[2]));
									mMil = (int)(double.Parse(n[2]) % 1 * 1000);
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