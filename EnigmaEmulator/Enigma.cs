using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// TODO:
///     -   Finish setting up plugboard
///     -   Add optional rotors for later machines - Logic is the same just extra steps
///     -   Make prettier UI?
///     -   Make is so users can encrypt & decrypt in the same instance of the application
/// </summary>
namespace EnigmaEmulator
{
    class Enigma
    {
        // Vairables holding the random data for the 
        // three different rotors
        private static string[] RotorOne = {"a", "b", "c", "d", "e", "f", "g",
                "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r",
            "s", "t", "u", "v", "w", "x", "y", "z"};
        private static string[] RotorTwo = {"a", "b", "c", "d", "e", "f", "g",
                "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r",
            "s", "t", "u", "v", "w", "x", "y", "z"};
        private static string[] RotorThree = {"a", "b", "c", "d", "e", "f", "g",
                "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r",
            "s", "t", "u", "v", "w", "x", "y", "z"};

        // Variables holding the original states of the rotors
        // used for decryption
        public static string[] OriginR1;
        public static string[] OriginR2;
        public static string[] OriginR3;

        // To maintain regular alphabet positioning
        private static string[] Alphabet = {"a", "b", "c", "d", "e", "f", "g",
                "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r",
            "s", "t", "u", "v", "w", "x", "y", "z"};

        // Holds settings for reflection
        private static string[] ReflectorSettings;

        // Holds settings for the plugboard
        private static string[] PlugBoardSettings;

        // Location string for the settings file
        private static string Settings;

        // Stores the encrypted message
        private static List<string> EncrMessage = new List<string>();

        // Random used to shuffle rotor arrays
        static Random random = new Random();

        // Encrypted letter
        static string EncrLetter;

        static string Ciphertext = "";

        // Sets the reflection pair settings
        static List<(string, string)> ReflecPairs = new List<(string, string)>();

        // Sets the plugboard pair settings
        static List<(string, string)> PlugPairs = new List<(string, string)>();

        // Count number of rotations for the rotors
        static int R1Count = 0;
        static int R2Count = 0;
        static int R3Count = 0;

        // Sets direction bool for the process
        static bool AfterReflec = false;

        /// <summary>
        /// Resets enigma back to original positions.
        /// Used for decryption
        /// </summary>
        static void Reset()
        {
            // Reset counters
            R1Count = 0;
            R2Count = 0;
            R3Count = 0;

            // Reset rotors
            RotorOne = OriginR1;
            RotorTwo = OriginR2;
            RotorThree = OriginR3;
        }

        /// <summary>
        /// Checks to see if the settings file is located in the right place
        /// should the user not use their own directory
        /// </summary>
        /// <returns></returns>
        static bool FindSettings()
        {
            // Get the current executing directory
            string currDirectory = Directory.GetCurrentDirectory();

            // Get parent of current executing directory
            DirectoryInfo directory = new DirectoryInfo(Directory.GetParent(currDirectory).FullName).Parent.Parent.Parent;

            //Get all of the files in the current directory
            FileInfo[] files = directory.GetFiles();

            // Seach each of the files looking for the 
            // Settings file
            foreach (var f in files)
            {
                if (f.Name.Contains("EnigmaSettings"))
                {
                    // Set the setting string
                    Settings = f.FullName;

                    // Return true if the file exists 
                    return true;
                }
            }

            // Return false if no such file exists
            return false;
        }

        /// <summary>
        /// Saves the ciphertext to its own text file
        /// </summary>
        static void SaveCiphertext(string ciphertext)
        {
            // Get path of solution
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.ToString();

            // Write ciphertext to file
            File.WriteAllText(path + "\\Ciphertext.txt", ciphertext);
        }

        /// <summary>
        /// The main execution method
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Check to see if a predefined settings files exists
            if (args.Length == 0)
            {
                //Find default settings file
                FindSettings();

                // Read in unique alphabet strings
                string[] settings = File.ReadAllLines(Settings);

                RotorOne = settings[0].Split(',');
                RotorTwo = settings[1].Split(',');
                RotorThree = settings[2].Split(',');
                OriginR1 = RotorOne;
                OriginR2 = RotorTwo;
                OriginR3 = RotorThree;
                ReflectorSettings = settings[3].Split(',');
                PlugBoardSettings = settings[4].Split(',');
                SetPlugReflection();
            }
            else
                // Set rotors randomly
                SetRotors();

            // Take string to be encrypted
            string input = Console.ReadLine();

            string[] letters = input.Split(' ');

            // If user wishes to decrypt, reset the rotors
            if (letters[0] == "/decrypt")
            {
                Reset();
            }

            // Make sure input is a letter
            var letter = new Regex("^[a-zA-Z ]*$");

            // Encrypt the letters
            foreach (string l in letters)
            {
                // Ignore user command
                if (!l.Contains("/"))
                {
                    foreach (char c in l)
                    {
                        if (letter.IsMatch(c.ToString()))
                        {
                            Encrypt(c.ToString().ToLower());
                            EncrMessage.Add(EncrLetter);
                            AfterReflec = false;
                            RotateRotors();
                        }
                    }
                    EncrMessage.Add(" ");
                }
            }

            // Builds encrypted message
            foreach (string e in EncrMessage)
            {
                Ciphertext += e;
            }

            // Save the ciphertext to its own file
            SaveCiphertext(Ciphertext);

            // Outputs encrypted message
            Console.WriteLine(Ciphertext.Trim().ToUpper());

            Console.ReadLine();
        }

        /// <summary>
        /// Sets the random rotor lettering from A to Z for all 3 rotors
        /// </summary>
        static void SetRotors()
        {
            Shuffle(RotorOne);
            Shuffle(RotorTwo);
            Shuffle(RotorThree);
        }

        /// <summary>
        /// Sets the reflection board for encryption process
        /// </summary>
        static void SetPlugReflection()
        {
            // Add values to reflection list
            foreach (string pair in ReflectorSettings)
            {
                string[] split = pair.Split('-');
                ReflecPairs.Add((split[0], split[1]));
            }

            // Add values to plugboard list
            foreach (string pair in PlugBoardSettings)
            {
                string[] split = pair.Split('-');
                PlugPairs.Add((split[0], split[1]));
            }
        }

        /// <summary>
        /// Sets the plug board switch
        /// Different from the reflector
        /// </summary>
        static void PlugBoard(string letter)
        {
            //// The reflected letter
            //string plugSwap = "";

            //// Set the plugboard letter switch
            //foreach (var s in PlugPairs)
            //{
            //    if (s.Item1.Trim() == letter)
            //    {
            //        plugSwap = s.Item2.Trim();
            //        break;
            //    }
            //    else if (s.Item2.Trim() == letter)
            //    {
            //        plugSwap = s.Item1.Trim();
            //        break;
            //    }
            //}
            //if (!AfterReflec)
            //    RotorOneFunc(plugSwap);
            //else
            //    EncrLetter = plugSwap;

            EncrLetter = letter;
        }

        /// <summary>
        /// Shuffles the arrays used for the rotors
        /// Only needed when not using predefined settings
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="alpha"></param>
        static void Shuffle<T>(T[] alpha)
        {
            // Set size of arrays (always 26)
            int size = 26;

            // Shuffle array to random unique value
            for (int i = 0; i < size; i++)
            {
                int rnd = i + random.Next(size - i);
                T t = alpha[rnd];
                alpha[rnd] = alpha[i];
                alpha[i] = t;
            }
        }

        /// <summary>
        /// Encrypt a letter
        /// </summary>
        static void Encrypt(string input)
        {
            //PlugBoard(input);

            RotorOneFunc(input);
        }

        /// <summary>
        /// Provides rotor one encryption and rotation
        /// </summary>
        /// <param name="letter"></param>
        static void RotorOneFunc(string letter)
        {
            if (!AfterReflec)
            {
                // Get position of letter relative to normal alphabet
                int position = Array.IndexOf(Alphabet, letter);

                string R1E = RotorOne[position];
                // Call the next rotor function for encryption
                RotorTwoFunc(R1E);
            }
            else
            {
                // Get position of letter relative to scrambled alphabet
                int position = Array.IndexOf(RotorOne, letter);

                string R1E = Alphabet[position];

                // Set encryption letter
                PlugBoard(R1E);
            }
        }

        /// <summary>
        /// Provides rotor two encryption and rotation
        /// </summary>
        /// <param name="letter"></param>
        static void RotorTwoFunc(string letter)
        {
            if (!AfterReflec)
            {
                // Get position of letter relative to normal alphabet
                int position = Array.IndexOf(Alphabet, letter);

                string R2E = RotorTwo[position];

                // Call the next rotor function for encryption
                RotorThreeFunc(R2E);
            }
            else
            {
                // Get position of letter relative to scrambled alphabet
                int position = Array.IndexOf(RotorTwo, letter);

                string R2E = Alphabet[position];

                // Call the RotorOneFunc on the wat back up
                RotorOneFunc(R2E);
            }
        }

        /// <summary>
        /// Provides rotor three encryption and rotation
        /// </summary>
        /// <param name="letter"></param>
        static void RotorThreeFunc(string letter)
        {
            if (!AfterReflec)
            {
                // Get position of letter relative to normal alphabet
                int position = Array.IndexOf(Alphabet, letter);

                string R3E = RotorThree[position];

                // Call the reflector method
                Reflector(R3E);
            }
            else
            {
                // Get position of letter relative to scrambled alphabet
                int position = Array.IndexOf(RotorThree, letter);

                string R3E = Alphabet[position];

                // Call the RotorTwoFunc on the way back up
                RotorTwoFunc(R3E);
            }
        }

        // Provides the reflection
        static void Reflector(string letter)
        {
            // The reflected letter
            string reflect = "";

            // Set the reflected letter
            foreach (var s in ReflecPairs)
            {
                if (s.Item1.Trim() == letter)
                {
                    reflect = s.Item2.Trim();
                    break;
                }
            }

            // Set direction bool
            AfterReflec = true;

            // Go back up the process
            RotorThreeFunc(reflect);
        }

        /// <summary>
        /// After encrypted letter has been given, 
        /// rotors must be advanced.
        /// </summary>
        static void RotateRotors()
        {
            // Reset the rotor count
            if (R1Count == 25)
                R1Count = 0;

            string[] R1Rotate = new string[RotorOne.Length];

            // Shift right
            for (int i = 0; i < 26; i++)
            {
                R1Rotate[(i + 1) % R1Rotate.Length] = RotorOne[i];
            }

            // Set the rotated array back to original
            RotorOne = R1Rotate;

            // Incrase the rotate count
            R1Count++;

            // Reset the rotor count
            if (R2Count == 25)
                R2Count = 0;
           
            // Shift rotors 
            if (R1Count == 25)
            {
                string[] R2Rotate = new string[RotorTwo.Length];

                // Shift right
                for (int i = 0; i < 26; i++)
                {
                    R2Rotate[(i + 1) % R2Rotate.Length] = RotorTwo[i];
                }

                // Set the rotated array back to original
                RotorTwo = R2Rotate;

                // Increase the rotate count
                R2Count++;
            }

            // Reset the rotor count
            if (R3Count > 26)
                R3Count = 0;

            if (R2Count == 25)
            {
                string[] R3Rotate = new string[RotorThree.Length];

                // Shift right
                for (int i = 0; i < 26; i++)
                {
                    R3Rotate[(i + 1) % R3Rotate.Length] = RotorThree[i];
                }

                // Set the rotated array back to original
                RotorThree = R3Rotate;

                // Increase the rotate count
                R3Count++;
            }
        }
    }
}