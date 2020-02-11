using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace DmvAppointmentScheduler
{
    class Program
    {
        public static Random random = new Random();
        public static List<Appointment> appointmentList = new List<Appointment>();
        static void Main(string[] args)
        {
            CustomerList customers = ReadCustomerData();
            TellerList tellers = ReadTellerData();
            Calculation(customers, tellers);
            OutputTotalLengthToConsole();

        }
        private static CustomerList ReadCustomerData()
        {
            string fileName = "CustomerData.json";
            string path = Path.Combine(Environment.CurrentDirectory, @"InputData\", fileName);
            string jsonString = File.ReadAllText(path);
            CustomerList customerData = JsonConvert.DeserializeObject<CustomerList>(jsonString);
            return customerData;

        }
        private static TellerList ReadTellerData()
        {
            string fileName = "TellerData.json";
            string path = Path.Combine(Environment.CurrentDirectory, @"InputData\", fileName);
            string jsonString = File.ReadAllText(path);
            TellerList tellerData = JsonConvert.DeserializeObject<TellerList>(jsonString);
            return tellerData;

        }
        static void Calculation(CustomerList customers, TellerList tellers)
        {

            // Your code goes here .....
            // Re-write this method to be more efficient instead of a assigning all customers to the same teller

            double duration = 0;

            // Keeps the type as key and the list of ids as value;
            Dictionary<string, List<string>> tellerMap = new Dictionary<string, List<string>>();

            // Store the duration taken by a teller for every appointment;(Teller Key; duration Value)
            Dictionary<Teller, double> durationMap = new Dictionary<Teller, double>();

            // Keeps the type of Customer as key and the list of Customers with that type as value;
            Dictionary<string, List<Customer>> customerMap = new Dictionary<string, List<Customer>>();

            // Update tellerMap
            foreach (Teller teller in tellers.Teller)
            {
                durationMap.Add(teller, 0);
                if (tellerMap.ContainsKey(teller.specialtyType))
                {
                    tellerMap[teller.specialtyType].Add(teller.id);
                }
                else
                {
                    tellerMap.Add(teller.specialtyType, new List<string> { teller.id });
                }
            }

            // Update CustomerMap
            foreach (Customer customer in customers.Customer)
            {
                if (customerMap.ContainsKey(customer.type))
                {
                    customerMap[customer.type].Add(customer);
                }
                else
                {
                    customerMap[customer.type] = new List<Customer> { customer };
                }
            }

            // we can sort the List in ascending if needed
            // customers.Customer = customers.Customer.OrderBy(e=>(Convert.ToDouble(e.duration))).ToList();

            // Till the last customer is processed : 
            while (customers.Customer.Count > 0)
            {
                // Subtract the duration taken by the appointment for all the tellers in the durationMap, as that time is elapsed for every Teller
                foreach (Teller t1 in durationMap.Keys.ToList())
                {
                    durationMap[t1] -= duration;
                }

                // Get the teller whose duration is the least, meaning this teller gets the nect customer.
                Teller availableTeller = durationMap.Where(e => e.Value == durationMap.Min(e1 => e1.Value)).First().Key;
                foreach (Teller teller in tellers.Teller)
                {
                    // Check if the teller who is available above is the teller we are looping through
                    if (availableTeller == teller)
                    {
                        // Check whether customerMap contains the type of the teller and has a proper value (List whose length>0) 
                        if (customerMap.ContainsKey(teller.specialtyType) && customerMap[teller.specialtyType].Count > 0)
                        {

                            // This is quite a simple solution. I randomly get a Customer belonging to that type and make a reservation.

                            // int index = random.Next(0, customerMap[teller.specialtyType].Count);
                            // get the random customer from the list of the Customer with that specific type.
                            //  Customer customer1 = customerMap[teller.specialtyType].Min(time => time.duration);

                            // A better solution would be to get the customer with the minimum processing time and assign it.
                            // Get the Customer whose duration is the least (may not make a big difference if assigned randomly)
                            Customer customer1 = customerMap[teller.specialtyType][0];
                            foreach (Customer locCustomer in customerMap[teller.specialtyType])
                            {
                                if (Convert.ToDouble(locCustomer.duration) < Convert.ToDouble(customer1.duration))
                                    customer1 = locCustomer;
                            }

                                // Schedule the appointment
                                var appointment = new Appointment(customer1, teller);
                                appointmentList.Add(appointment);
                                // get the duration of the appointment
                                duration = appointment.duration;
                                // update the duratioin of the teller in the durationMap
                                durationMap[teller] = duration;
                                
                                // now remove the customer from our original Customer List;
                                // a safe way would be to clone this list and perform actions on that List(In order to not change the actual data).
                                customers.Customer.Remove(customer1);
                                // and also remove it from the dictionary;
                                customerMap[teller.specialtyType].Remove(customer1);
                                break;
                      
                        }
                        // else if there are no customer with the tellers type, assign a random customer to that teller.
                        else
                        {
                            // random Customer
                            // get a random customer from our Customer List
                            int index = random.Next(0, customers.Customer.Count);
                            Customer customer1 = customers.Customer[index];

                            // Schedule an appointment 
                            var appointment = new Appointment(customer1, teller);
                            appointmentList.Add(appointment);
                            // get the duration and update the durationMap
                            duration = appointment.duration;
                            durationMap[teller] = duration;

                            // Remove it from the CustomerMap by looking up the type 
                            customerMap[customer1.type].Remove(customer1);
                            // Remove it from Our main Customer List
                            customers.Customer.Remove(customer1);
                            break;
                        }
                    }
                }
            }

            // By assigning every customer as according to the list to every immediate available Teller.

            // foreach (Customer customer in customers.Customer)
            // {
            //     foreach (Teller t1 in durationMap.Keys.ToList())
            //     {
            //         durationMap[t1] -= duration;
            //     }
            // 
            //     Teller avail = durationMap.Where(e => e.Value == durationMap.Min(e1 => e1.Value)).First().Key;
            //     foreach (Teller teller in tellers.Teller)
            //     {
            //         if (tellerMap.ContainsKey(customer.type))
            //         {
            //             List<string> localtellerList = tellerMap[customer.type];
            //             foreach (string localTeller in localtellerList)
            //             {
            //                 avail = tellers.Teller.Find(
            //                   delegate (Teller locteller)
            //                 {
            //                     return locteller.id == localTeller;
            //                 });
            //
            //                 var appointment = new Appointment(customer, avail);
            //                 appointmentList.Add(appointment);
            //                 duration = appointment.duration;
            //                 durationMap[avail] = duration;
            //                 break;
            //             }
            //             break;
            //         }
            //         else
            //         {
            //             var appointment = new Appointment(customer, avail);
            //             appointmentList.Add(appointment);
            //             duration = appointment.duration;
            //             durationMap[avail] = duration;
            //             break;
            //         }
            // 
            //    }
            //
            // }

            // Simple but not optimal, just assigning it to random
            // foreach (Customer customer in customers.Customer)
            // { 
            //     var tellerIndex = random.Next(0,tellers.Teller.Count);
            //     var appointment = new Appointment(customer, tellers.Teller[tellerIndex]);
            //     appointmentList.Add(appointment);
            // }
        }
        static void OutputTotalLengthToConsole()
        {
            var tellerAppointments =
                from appointment in appointmentList
                group appointment by appointment.teller into tellerGroup
                select new
                {
                    teller = tellerGroup.Key,
                    totalDuration = tellerGroup.Sum(x => x.duration),
                };
            var max = tellerAppointments.OrderBy(i => i.totalDuration).LastOrDefault();
            Console.WriteLine("Teller " + max.teller.id + " will work for " + max.totalDuration + " minutes!");
        }

    }
}
