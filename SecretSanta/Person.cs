namespace SecretSanta
{
    /// <summary>
    /// The class that holds the information about the person.
    /// </summary>
    class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public Person Giftee { get; set; }
    }
}
