using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Wizard.Modules
{
    public class Grimoire
    {
        // Store data in %APPDATA%\Wizard\
        private static readonly string DataFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Wizard");

        private static readonly string GrimoirePath =
            Path.Combine(DataFolder, "grimoire.json");

        public class Note
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Body { get; set; }
            public DateTime Timestamp { get; set; }
        }

        private List<Note> notes = new List<Note>();

        public Grimoire()
        {
            LoadOrCreate();
        }

        // ------------------------------------------------------------
        // LOAD OR CREATE
        // ------------------------------------------------------------
        private void LoadOrCreate()
        {
            try
            {
                Directory.CreateDirectory(DataFolder);

                if (!File.Exists(GrimoirePath))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("📜 The ancient Grimoire was missing… forging a new tome.");
                    Console.ResetColor();

                    Save();
                    return;
                }

                string json = File.ReadAllText(GrimoirePath);
                notes = JsonSerializer.Deserialize<List<Note>>(json) ?? new List<Note>();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("⚠️ The Grimoire was corrupted or unreadable! Creating a fresh one.");
                Console.WriteLine($"Reason: {ex.Message}");
                Console.ResetColor();

                notes = new List<Note>();
                Save();
            }
        }

        // ------------------------------------------------------------
        // SAVE
        // ------------------------------------------------------------
        private void Save()
        {
            try
            {
                Directory.CreateDirectory(DataFolder);

                string json = JsonSerializer.Serialize(notes, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(GrimoirePath, json);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("⚠️ Failed to inscribe the Grimoire.");
                Console.WriteLine($"Reason: {ex.Message}");
                Console.ResetColor();
            }
        }

        // ------------------------------------------------------------
        // MENU
        // ------------------------------------------------------------
        public void Run()
        {
            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("📖 THE WIZARD'S GRIMOIRE");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("[1] Inscribe a new note");
                Console.WriteLine("[2] Read all notes");
                Console.WriteLine("[3] Search the Grimoire");
                Console.WriteLine("[4] Burn a note");
                Console.WriteLine("[5] Return to the chamber");
                Console.WriteLine();
                Console.Write("Your choice: ");

                string input = Console.ReadLine();

                switch (input)
                {
                    case "1": AddNote(); break;
                    case "2": ReadNotes(); break;
                    case "3": SearchNotes(); break;
                    case "4": DeleteNote(); break;
                    case "5": return;
                    default:
                        Console.WriteLine("The runes do not recognise that choice.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        // ------------------------------------------------------------
        // ADD NOTE
        // ------------------------------------------------------------
        private void AddNote()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("✍️ Speak your wisdom, mortal. I shall etch it into the eternal parchment.");
            Console.ResetColor();

            Console.Write("Title: ");
            string title = Console.ReadLine();

            Console.Write("Body: ");
            string body = Console.ReadLine();

            int newId = notes.Count == 0 ? 1 : notes.Max(n => n.Id) + 1;

            notes.Add(new Note
            {
                Id = newId,
                Title = title,
                Body = body,
                Timestamp = DateTime.Now
            });

            Save();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✨ Your words have been inscribed.");
            Console.ResetColor();
            Console.ReadKey();
        }

        // ------------------------------------------------------------
        // READ NOTES
        // ------------------------------------------------------------
        private void ReadNotes()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("📜 Reading the ancient pages…");
            Console.ResetColor();
            Console.WriteLine();

            if (notes.Count == 0)
            {
                Console.WriteLine("The Grimoire is empty. No wisdom has yet been recorded.");
                Console.ReadKey();
                return;
            }

            foreach (var note in notes)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"[{note.Id}] {note.Title} ({note.Timestamp})");
                Console.ResetColor();
                Console.WriteLine(note.Body);
                Console.WriteLine(new string('-', 40));
            }

            Console.ReadKey();
        }

        // ------------------------------------------------------------
        // SEARCH NOTES
        // ------------------------------------------------------------
        private void SearchNotes()
        {
            Console.Clear();
            Console.Write("🔍 Speak the keyword you seek: ");
            string query = Console.ReadLine()?.ToLower() ?? "";

            var results = notes.Where(n =>
                n.Title.ToLower().Contains(query) ||
                n.Body.ToLower().Contains(query)).ToList();

            Console.WriteLine();

            if (results.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No matching runes were found within the Grimoire.");
                Console.ResetColor();
                Console.ReadKey();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✨ {results.Count} matching entries found:");
            Console.ResetColor();
            Console.WriteLine();

            foreach (var note in results)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"[{note.Id}] {note.Title} ({note.Timestamp})");
                Console.ResetColor();
                Console.WriteLine(note.Body);
                Console.WriteLine(new string('-', 40));
            }

            Console.ReadKey();
        }

        // ------------------------------------------------------------
        // DELETE NOTE
        // ------------------------------------------------------------
        private void DeleteNote()
        {
            Console.Clear();
            Console.Write("🔥 Which note shall be cast into the flames? Enter ID: ");

            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("The runes reject your input.");
                Console.ReadKey();
                return;
            }

            var note = notes.FirstOrDefault(n => n.Id == id);
            if (note == null)
            {
                Console.WriteLine("No such page exists in the Grimoire.");
                Console.ReadKey();
                return;
            }

            Console.Write($"Are you certain you wish to burn '{note.Title}'? (y/n): ");
            if (Console.ReadLine()?.ToLower() == "y")
            {
                notes.Remove(note);
                Save();

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("🔥 The page has been consumed by arcane fire.");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("The page remains intact.");
            }

            Console.ReadKey();
        }
    }
}
