using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace BreakingEnigma
{
    class Codebreaker
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

        // To maintain regular alphabet positioning
        private static string[] Alphabet = {"a", "b", "c", "d", "e", "f", "g",
                "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r",
            "s", "t", "u", "v", "w", "x", "y", "z"};

        // Variables holding the original states of the rotors
        // used for decryption
        public static string[] OriginR1 = {"a", "b", "c", "d", "e", "f", "g",
                "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r",
            "s", "t", "u", "v", "w", "x", "y", "z"};
        public static string[] OriginR2 = {"a", "b", "c", "d", "e", "f", "g",
                "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r",
            "s", "t", "u", "v", "w", "x", "y", "z"};
        public static string[] OriginR3 = {"a", "b", "c", "d", "e", "f", "g",
                "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r",
            "s", "t", "u", "v", "w", "x", "y", "z"};

        // Stores the encrypted message
        private static List<string> EncrMessage = new List<string>();

        // Random used to shuffle rotor arrays
        static Random random = new Random();

        // Encrypted letter
        static string EncrLetter;

        // Holds cipher string
        static string Plaintext = "";

        // Count number of rotations for the rotors
        static int R1Count = 0;
        static int R2Count = 0;
        static int R3Count = 0;

        // Sets direction bool for the process
        static bool AfterReflec = false;

        // Sets the reflection pair settings
        static List<(string, string)> ReflecPairs = new List<(string, string)>();

        // Sets the plugboard pair settings
        static List<(string, string)> PlugPairs = new List<(string, string)>();

        // String for the pair match
        static string posMatch = "";

        // int for the reflector pair counter
        static int counter = 0; 

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
        /// The main execution method
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Take string to be encrypted
            string input = Console.ReadLine();

            string[] letters = input.Split(' ');

            // Make sure input is a letter
            var letter = new Regex("^[a-zA-Z ]*$");

            // Checks to see if correct letter has be decrypted
            bool found = false;

            //
            bool solved = false;

            // Plain text is known beforehand
            while (Plaintext != "hello")
            {
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
                                // Test every single character in alphabet EXCEPT letter
                                // itself
                                foreach (var alpha in RotorOne)
                                {
                                    while (c.ToString() != EncrLetter)
                                    {                                 
                                        // Attempt decryption
                                        if (alpha != c.ToString())
                                            Decrypt(alpha);

                                        // If the letter is found add it to the message
                                        if (c.ToString() == EncrLetter)
                                        {
                                            AfterReflec = false;
                                            EncrMessage.Add(" ");
                                            Reset();
                                            break;
                                        }

                                        // If all reflector combinations have been checked, move on to the next rotor position
                                        if (counter == 25)
                                        {
                                            // Rotate rotors and try again
                                            AfterReflec = false;
                                            RotateRotors();
                                            counter = 0;
                                        }
                                    }
                                }

                                // Break if the correct letter is found
                                if (found)
                                    break;
                            }
                        }
                    }
                }

                // Builds encrypted message
                foreach (string e in EncrMessage)
                {
                    Plaintext += e;
                }

                // Outputs encrypted message
                Console.WriteLine(Plaintext.Trim().ToUpper());
            }

            Console.ReadLine();
        }


        /// <summary>
        /// Decrypt a letter
        /// </summary>
        static void Decrypt(string input)
        {
            //PlugBoard(input);

            RotorOneFunc(input);
        }


        /// <summary>
        /// Sets the plug board switch
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
            // Set direction bool
            AfterReflec = true;

            // Set reflect letter 
            posMatch = OriginR1[counter];

            // Incr 1 if letter is the same (letter)
            // cannot be the same
            if (letter == posMatch)
            {
                counter++;
                posMatch = OriginR1[counter];
            }

            // Go back up the process
            RotorThreeFunc(posMatch);
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
