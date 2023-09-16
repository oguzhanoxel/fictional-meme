using LogisticsManagementConsoleApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// username & passwords

// ""              ""
// "admin"         "123"
// "venenatis"     "123456"
// "ultrices"      "123456" 
// "vehicula"      "123456" 
// "vestibulum"    "123456" 
namespace LogisticsManagementConsoleApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            InmemoryDB db = new InmemoryDB();
            UserManagement userManagement = new UserManagement(db);
            bool isWorking = true;


            while (isWorking)
            {
                Console.Write("Username: ");
                string username = Console.ReadLine();

                Console.Write("Password: ");
                string password = Console.ReadLine();

                User currentUser = userManagement.Login(username, password);
                bool isLogin = (currentUser != null) ? true:false;

                while (isLogin)
                {
                    Console.WriteLine("\n'1' Display All Users");
                    Console.WriteLine("'2' Display My Profile");
                    Console.WriteLine("'3' Start Shift");
                    Console.WriteLine("'4' Finish Shift");
                    Console.WriteLine("'5' Test");
                    Console.WriteLine("'9' Change Zone Info Id");
                    Console.WriteLine("'-1' Logout");
                    Console.Write("\nEnter a number: ");
                    
                    if(!int.TryParse(Console.ReadLine(), out int menuNumber))
                    {
                        Console.WriteLine("Invalid value.");
                    }
                    // int menuNumber = Convert.ToInt32(Console.ReadLine());

                    switch (menuNumber)
                    {
                        case 1:
                            Console.WriteLine();
                            userManagement.DisplayUsers();
                            Console.WriteLine("\npress a key to return");
                            Console.ReadKey();
                            break;
                        case 2:
                            Console.WriteLine();
                            userManagement.DisplayCurrentUser(currentUser);
                            Console.WriteLine("\npress a key to return");
                            Console.ReadKey();
                            break;
                        case 3:
                            userManagement.StartShift(currentUser);
                            Console.WriteLine("\npress a key to return");
                            Console.ReadKey();
                            break;
                        case 4:
                            userManagement.FinishShift(currentUser);
                            Console.WriteLine("\npress a key to return");
                            Console.ReadKey();
                            break;
                        case 5:
                            Console.WriteLine();
                            Test.Run();
                            Console.WriteLine("\npress a key to return");
                            Console.ReadKey();
                            break;
                        case 9:
                            Console.Write("Enter Zone Info Id: ");
                            string info = Console.ReadLine();
                            db.SetTimeZoneInfo(info);
                            Console.WriteLine("\npress a key to return");
                            Console.ReadKey();
                            break;
                        case -1:
                            isLogin = false;
                            break;
                        default:
                            Console.WriteLine("Invalid number.");
                            break;
                    }
                }
            }
        }
    }
    
    public static class Test
    {
        public static void Run()
        {
            User user = new User("myFirstName", "myLastName", "username", "123", "sample@example.com", null, null);
            InmemoryDB db = new InmemoryDB();
            UserManagement userManagement = new UserManagement(db);
            string info = "Tokyo Standard Time";
            db.SetTimeZoneInfo(info);
            Console.WriteLine(info);

            Thread.Sleep(1000);

            Console.WriteLine();
            db.Users.Add(user);
            userManagement.DisplayCurrentUser(user);

            Thread.Sleep(1000);

            Console.WriteLine();
            userManagement.StartShift(user);
            userManagement.DisplayCurrentUser(user);

            Thread.Sleep(1000);

            Console.WriteLine();
            user.WorkingFinishTime = db.GetCurretTime().AddHours(7.5);
            userManagement.DisplayCurrentUser(user);

        }
    }

    public class UserManagement
    {
        private InmemoryDB db;

        public UserManagement(InmemoryDB db)
        {
            this.db = db;
        }

        public User Login(string username, string password)
        {
            foreach (User user in db.Users) {
                if (username == user.Username && password == user.Password)
                {
                    Console.WriteLine("Login Successful.\n");
                    return user;
                }
            }
            Console.WriteLine("Username or password is wrong.\n");
            return null;
        }

        public bool Logout ()
        {
            return false;
        }

        public void StartShift(User user)
        {
            db.SetUserWorkingStartTime(user);
            Console.WriteLine($"Start Time: {db.GetUser(user.Username).WorkingStartTime}");
        }

        public void FinishShift(User user)
        {
            db.SetUserWorkingFinishTime(user);
            Console.WriteLine($"Finish Time: {db.GetUser(user.Username).WorkingFinishTime}");
        }

        public double CalculateDailyWage(User user)
        {
            if (!user.WorkingStartTime.HasValue) {
                return 0;
            }
            return 50 * (GetTotalWorkingDuration(user)/60);
        }

        public double GetTotalWorkingDuration(User user)
        {
            TimeSpan totalWorkingDuration;

            if (!user.WorkingStartTime.HasValue)
            {
                return 0;
            }

            if (user.WorkingFinishTime.HasValue)
            {
                totalWorkingDuration = user.WorkingFinishTime.Value - user.WorkingStartTime.Value;
            } else {
                totalWorkingDuration = db.GetCurretTime() - user.WorkingStartTime.Value;
            }

            return totalWorkingDuration.TotalMinutes;
        }

        public TimeSpan GetTotalWorkingHours(User user)
        {
            TimeSpan totalWorkingDuration = TimeSpan.FromMinutes(GetTotalWorkingDuration(user));

            return totalWorkingDuration;
        }

        public void DisplayUsers() { 
            foreach (User user in db.Users)
            {
                Console.WriteLine($"Username: {user.Username} / email: {user.Email}" +
                    $"\nStart Time: {user.WorkingStartTime} / FinishTime: {user.WorkingFinishTime} / Working Hours: {GetTotalWorkingHours(user).ToString(@"hh\:mm")}" +
                    $"\nDaily Wage: {CalculateDailyWage(user).ToString("C0")}");
            }
        }

        public void DisplayCurrentUser(User user)
        {
            Console.WriteLine($"Username: {user.Username} / email: {user.Email}" +
                $"\nStart Time: {user.WorkingStartTime} / FinishTime: {user.WorkingFinishTime} / Working Hours: {GetTotalWorkingHours(user).ToString(@"hh\:mm")}" +
                $"\nDaily Wage: {CalculateDailyWage(user).ToString("C0")}");
        }
    }

    public class InmemoryDB
    {
        private TimeZoneInfo info;
        private DateTime currentTime;

        public List<User> Users { get; set; }

        public InmemoryDB() {
            Users = new List<User>();

            Users.Add(new User("a", "a", "", "a@example.com", "", null, null));
            Users.Add(new User("aenean", "venenatis", "venenatis", "venenatis@example.com", "123456", null, null));
            Users.Add(new User("fermentum", "ultrices", "ultrices", "ultrices@example.com", "123456", null, null));
            Users.Add(new User("admin", "admin", "admin", "admin@example.com", "123", null, null));
            Users.Add(new User("velit", "vehicula", "vehicula", "vehicula@example.com", "123456", null, null));
            Users.Add(new User("vestibulum", "vestibulum", "vestibulum", "vestibulum@example.com", "123456", null, null));

            info = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            currentTime = TimeZoneInfo.ConvertTime(DateTime.Now, info);
        }

        public DateTime GetCurretTime()
        {
            return currentTime;
        }

        public User GetUser(string username)
        {
            return Users.Find(u => u.Username == username);
        }

        public void SetTimeZoneInfo(string info)
        {
            foreach(TimeZoneInfo timeZoneInfo in TimeZoneInfo.GetSystemTimeZones())
            {
                if(timeZoneInfo.Id == info)
                {
                    this.info = TimeZoneInfo.FindSystemTimeZoneById(info);
                    currentTime = TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo);
                    return;
                }
            }
            Console.WriteLine("Invalid timezone id.");
        }

        public void SetUserWorkingStartTime(User user)
        {
            Users.Find(u => user.Username == u.Username)
                .WorkingStartTime = currentTime;
        }

        public void SetUserWorkingFinishTime(User user)
        {
            Users.Find(u => user.Username == u.Username)
                .WorkingFinishTime = currentTime;
        }
    }

    public class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime? WorkingStartTime { get; set; }
        public DateTime? WorkingFinishTime { get; set; }

        public User(string firstName, string lastName, string username, string email, string password, DateTime? workingStartTime, DateTime? workingFinishTime)
        {
            FirstName = firstName;
            LastName = lastName;
            Username = username;
            Email = email;
            Password = password;
            WorkingStartTime = workingStartTime;
            WorkingFinishTime = workingFinishTime;
        }
    }
}
