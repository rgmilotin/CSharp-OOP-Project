Matcha Store Network (C# OOP Project)

A C#/.NET application that manages a small network of matcha shops (“matcheries”), allowing clients to buy matcha products, reserve tables, and track transaction history, while admins manage shops, reservation types, and monitor overall activity.

This project is designed to demonstrate clean Object-Oriented Design, data persistence, robust error handling, and (for the full requirements) modern .NET setup via GenericHost, Dependency Injection, and ILogger.

Features
Authentication & Roles

Login-required access to all functionalities

Two user types:

Admin

Client

Role-based menus and actions

Admin Features

Manage Matcheries (Shops)

Create / edit / delete matcheries

Configure shop info: schedule, location (optional), capacity

Manage Reservation Types

Create / edit / delete reservation types (e.g., Family, Friends, Birthday)

Configure:

price

limitations (e.g., min/max people, cancellation rules)

benefits (e.g., free dessert, priority seating)

Transaction History Management

Create and modify transactions

Associate transactions with a specific client

Activity Monitoring

Aggregated info:

number of active reservations

occupancy rate per shop (e.g., reserved seats / total capacity)

Client Features

Browse Shops & Menus

View matcheries list

View available menu items/products

Buy Matcha Products

Purchase matcha beverages and/or matcha desserts

Creates a transaction entry

Reserve a Table

View available time slots based on schedule + capacity

Reserve seats within capacity constraints

View History

Active and past reservations

Transaction history

Cancel Reservations

Cancellation allowed only according to the defined rules (e.g., time window, penalties)

Persistence (File-Based)

The application automatically saves and loads required data using files, for example:

users

matcheries

menu items

reservation types

reservations

transactions

Error Handling Requirements

The app must handle cases such as:

missing files (first run / deleted files)

invalid/corrupted data

validation errors (e.g., capacity exceeded, invalid reservation time)

Technical Requirements Mapping
Target Grade 8 (Core)

✅ Clear OO model (encapsulation, inheritance, polymorphism, composition)

✅ File-based persistence (save/load)

✅ Exception handling + robust error management

✅ Documentation + progress history (commits + README + optional CHANGELOG)

Target Grade 10 (Advanced)

✅ .NET GenericHost for app configuration and dependency management

✅ ILogger for logging errors and user actions

✅ More complex model with immutable types for core entities/business logic

✅ A Coordinator / Aggregate (DDD-style) controlling coherent updates:

no public setters

no direct state mutation from outside

changes happen through domain methods (e.g., ReserveTable(...), CancelReservation(...))

Optional Bonus (if time)

SQL database persistence as an alternative to files

GUI (WinForms/WPF)

Layered architecture (e.g., Domain / Application / Infrastructure / UI)

Unit tests
