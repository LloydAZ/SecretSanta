using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace SecretSanta
{
    /// <summary>
    /// Secret Santa - http://rubyquiz.com/quiz2.html
    /// My C# take on the secret Santa quiz from Ruby Quiz.
    /// </summary>
    class Program
    {
        /// <summary>
        /// We'll use the same collection that is represented in the quiz.
        /// </summary>
        public static string[,] personList = new string[7, 3]
        {
            { "Luke", "Skywalker", "<luke@theforce.net>" },
            { "Leia", "Skywalker", "<leia@therebellion.org>" },
            { "Toula", "Portokalos", "<toula@manhunter.org>" },
            { "Gus", "Portokalos", "<gus@weareallfruit.net>" },
            { "Bruce", "Wayne", "<bruce@imbatman.com>" },
            { "Virgil", "Brigman", "<virgil@rigworkersunion.org>" },
            { "Lindsey", "Brigman", "<lindsey@iseealiens.net>" }
        };

        static void Main(string[] args)
        {
            List<Person> people = new List<Person>();
            int numPlayers = personList.GetLength(0);

            // Build up a list of objects of type Person from the personList array.
            for (int x = 0; x < numPlayers; x++)
            {
                Person myPerson = new Person();
                myPerson.FirstName = personList[x, 0];
                myPerson.LastName = personList[x, 1];
                myPerson.EmailAddress = personList[x, 2];

                people.Add(myPerson);
            }

            // Assign each player a giftee that they will be the secret Santa for.
            AssignSecretSanta(people);

            // Send an eMail to all of the players with the name of their giftee.
            EmailPlayers(people);

            Console.Write("<Press any key to end>");

            Console.Read();
        }

        /// <summary>
        /// Assign each person in the list a giftee that they will be a secret Santa for.
        /// </summary>
        /// <param name="people">The list of people playing (List<Person>)</param>
        private static void AssignSecretSanta(List<Person> people)
        {
            // The number of people assigned a giftee,
            int assigned = 0;

            // The number of people in the list.
            int listLength = people.Count();

            // A hash set containing the index of someone who has already been selected as a giftee.
            HashSet<int> selectedValues = new HashSet<int>();

            // In some cases we may need to resuffle the names and start again.
            bool reshuffle = false;

            // Clear out all of the giftees.
            foreach (Person person in people)
            {
                person.Giftee = null;
            }

            // Loop through every person in the list.
            for (int x = 0; x < people.Count; x++)
            {
                // We're on the last person, and they were not selected by someone else.
                // We need to resuffle and try again.
                if ((x == (people.Count - 1)) && (!selectedValues.Contains(x)))
                {
                    Console.WriteLine("The last person wasn't selected by someone else.  Reshuffle...");
                    reshuffle = true;
                    break;
                }

                // This is the person that we are going to assign a giftee to.
                Person person = people[x];

                // This is the person that will receive the gift.
                Person giftee = new Person();

                // Loop until the person has been assigned a giftee.
                while (person.Giftee == null)
                {
                    // Generate a random number between zero and the number of players in the list.
                    Random random = new Random();
                    int randomNumber = random.Next(0, listLength);

                    // Test to see if we are on the last person in the list.
                    if (x == people.Count)
                    {
                        // Loop through each person in the list to see if they have already been selected.
                        // We are looking to shortcut the random number process at this point as there should
                        // only be one person left who has not been selected.
                        for (int i = 0; i < people.Count; i++)
                        {
                            // If the person has not been selected, set their index as the random number.
                            if (!selectedValues.Contains(i))
                            {
                                randomNumber = i;
                            }
                        }
                    }

                    // The giftee and the person cannot be the same.  We also do not want to
                    // select someone that has already been selected.
                    while (randomNumber == x)
                    {
                        randomNumber = random.Next(0, listLength - 1);
                    }

                    // If the random number has not already been selected, then continue
                    // otherwise re-roll a new number.
                    if (!selectedValues.Contains(randomNumber))
                    {
                        giftee = people[randomNumber];

                        // If the person's last name is not the same as the giftee's last name,
                        // then assign the giftee to the person.
                        if (person.LastName != giftee.LastName)
                        {
                            person.Giftee = new Person();
                            person.Giftee = giftee;
                            selectedValues.Add(randomNumber);
                            assigned++;
                        }
                        else if ((person.LastName == giftee.LastName) && (assigned == x))
                        {
                            // If the last person selects someone with the same last name, we need to 
                            // reshuffle and start again.
                            Console.WriteLine("The last person selected a family member.  Reshuffle...");
                            reshuffle = true;
                            break;
                        }
                    }
                }

                // If reshuffle is set to true, the break out of the loop.
                if (reshuffle)
                {
                    break;
                }
            }

            // If reshuffle is set to true, we will need to recursively call this method again until
            // all of the people playing have been selected.
            if (reshuffle)
            {
                AssignSecretSanta(people);
            }

            return;
        }

        /// <summary>
        /// Email each of the players with the name of the person that they will need to
        /// purchase a gift for.
        /// </summary>
        /// <param name="people">The list of people playing (List<Person>)</param>
        private static void EmailPlayers(List<Person> people)
        {
            Console.WriteLine();

            foreach (Person person in people)
            {
                string message = String.Format("Dear {0},\r\n\r\nThe computer selected you to be the Secret Santa for {1} {2}.",
                    person.FirstName,
                    person.Giftee.FirstName,
                    person.Giftee.LastName);

                message = String.Format("{0}\r\n\r\nSincerely,\r\nSecret Santa Headquarters", message);

                string fromAddress = "<santa@sshq.com>";
                string subject = "Secret Santa";

                // The code for sending the eMail came from: 
                // https://www.codeproject.com/Tips/301836/Simple-SMTP-E-Mail-Sender-in-Csharp-Console-applic
                MailAddress to = new MailAddress(person.EmailAddress);
                MailAddress from = new MailAddress(fromAddress);

                MailMessage mail = new MailMessage(from, to);
                mail.Subject = subject;

                mail.Body = message;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;

                smtp.Credentials = new NetworkCredential("username@domain.com", "password");
                smtp.EnableSsl = true;

                Console.WriteLine("Sending email...");

                // I have disabled actually sending the eMail.  Instead I will just display the 
                // message on the screen for demonstration purposes.
                //smtp.Send(mail);

                Console.WriteLine();
                Console.WriteLine(String.Format("To: {0}", person.EmailAddress));
                Console.WriteLine(String.Format("From: {0}", fromAddress));
                Console.WriteLine(String.Format("Subject: {0}", subject));
                Console.WriteLine();
                Console.WriteLine(message);
                Console.WriteLine();
                Console.WriteLine("-----------------------------------------------------------------------------");
            }
        }
    }
}
