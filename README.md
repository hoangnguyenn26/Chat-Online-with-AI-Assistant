# ChatApp – Real-time Chat Application with AI Assistant

A modern, real-time chat application built using **ASP.NET Core** for the backend and **Angular** for the frontend. This project is a comprehensive example of building a full-stack Single Page Application (SPA), featuring OAuth 2.0 authentication, real-time communication via SignalR, and integration with third-party AI services.

![.NET](https://img.shields.io/badge/.NET-8-blueviolet.svg)
![Angular](https://img.shields.io/badge/Angular-17+-red.svg)
![SignalR](https://img.shields.io/badge/SignalR-real--time-brightgreen.svg)
![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)

---

## ✨ Core Features (MVP)

- **OAuth 2.0 Authentication**  
  Secure login/register with Google. Backend manages session using JWT.

- **Real-time User Presence**  
  Instantly displays online/offline status using SignalR.

- **1-on-1 Private Messaging**  
  Enables real-time chat between two users.

- **Chat History Loading (Lazy Loading)**  
  Load older messages on demand when scrolling up.

- **AI Assistant Integration**  
  Call OpenAI's API to ask questions and receive intelligent responses within the chat.

---

## 🚀 Technologies Used

### Backend – ASP.NET Core Web API

- **.NET 8**, **ASP.NET Core**
- **Entity Framework Core 8**
- **SignalR**
- **JWT Authentication**
- **Google OAuth 2.0**
- **OpenAI API**
- **Clean/Onion Architecture**
- **Repository & Unit of Work Patterns**
- **AutoMapper**
- **FluentValidation**
- **Serilog**

### Frontend – Angular SPA

- **Angular 17+** (Standalone Components)
- **TypeScript**
- **RxJS**
- **Angular Material**
- **@microsoft/signalr**
- **SCSS**
- **Angular Router**
- **HTTP Interceptor** (for auto-attaching JWT)
- **Auth Guard**
