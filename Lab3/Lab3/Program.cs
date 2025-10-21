using System;
using System.IO;
using System.Text;

namespace E3
{
    struct Contact
    {
        public string Name;
        public string Num;
    }

    class Program
    {
        const int MAX_CONTACTS_COUNT = 100;

        static Contact[] _contacts = new Contact[MAX_CONTACTS_COUNT];

        static int _contactsCount = 0;

        static void Add(string name, string num)
        {
            _contacts[_contactsCount++] = new Contact
            {
                Name = name,
                Num = num
            };
        }

        static string Find(string name)
        {
            for (int i = 0; i < _contactsCount; i++)
                if (_contacts[i].Name == name)
                    return _contacts[i].Num;

            return null;
        }

        static void Main()
        {
            while (true)
            {
                Console.Clear();
                Console.Write("Command (a: Add; f: Find; q: Quit):");
                var command = Console.ReadKey().KeyChar;
                Console.WriteLine();

                switch (command)
                {
                    case 'a':
                        {
                            Console.Write("Name:");
                            var name = Console.ReadLine();

                            Console.Write("Num:");
                            var num = Console.ReadLine();

                            Add(name, num);
                        }
                        break;

                    case 'f':
                        {
                            Console.Write("Name:");
                            var name = Console.ReadLine();

                            var num = Find(name);

                            if (num != null)
                                Console.WriteLine($"Num: {num}");
                            else
                                Console.WriteLine("Not found.");

                            Console.ReadLine();
                        }
                        break;

                    case 'q':

                        return;
                }
            }
        }
    }
}
