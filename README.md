# Captive Portal Management System

## Overview
This project is a full-featured Captive Portal Management System for West Texas A&M University, built with ASP.NET Core Razor Pages, Entity Framework Core, and SQLite. It provides a secure Wi-Fi onboarding experience for users and a comprehensive administrative interface for managing advertisements, campaigns, users, and analytics.

## Key Features
- **Captive Portal**: User-friendly Wi-Fi access page with email collection and terms acceptance
- **Admin Authentication**: Secure login for administrators (ASP.NET Identity)
- **Advertisement Management**: Upload, manage, and display ads on the portal
- **Campaigns**: Group ads into campaigns for better organization and analytics
- **Bulk Upload**: Upload multiple ads at once with shared settings
- **Advanced Filtering**: Search and filter ads by status, campaign, location, and tags
- **Enable/Disable Ads**: Toggle ad visibility individually or in bulk
- **Analytics**: Track ad views, clicks, and click-through rates
- **User Tracking**: Record user connections, locations, and session data
- **RESTful API**: Endpoints for ad management and analytics
- **Bootstrap 5 UI**: Modern, responsive, and WTAMU-branded admin interface

## What Has Been Implemented So Far

### ✅ PBI #6 - Advertisement Display on Portal
- Ads are displayed to users on the captive portal page
- Only active, scheduled, and non-expired ads are shown
- Click tracking and view analytics implemented

### ✅ PBI #7 - Advertisement Upload and Management
- Enhanced Advertisement model with campaign, priority, tags, status, analytics fields
- Campaign model and database relationship
- Bulk upload page for multiple ads
- Advanced admin interface with filtering, campaign selection, and bulk operations
- Entity Framework migration for all schema changes

### ✅ PBI #8 - Advertisement Control (Enable/Disable)
- Toggle buttons for enabling/disabling ads (individually and in bulk)
- Visual status indicators (active/inactive badges)
- Bulk operations for status and deletion
- RESTful API endpoints for all control actions

## Remaining PBIs (Next Steps)
- **User Management & Analytics**: Admin interface for user data, search, and session management
- **Campaign Management UI**: Full CRUD and analytics for campaigns
- **System Settings**: Portal and Wi-Fi configuration, branding, and rules
- **Reports & Analytics**: Usage, ad performance, and exportable reports
- **WiFi Access Control**: Manual access granting/revoking, device management

## Technologies Used
- ASP.NET Core 10.0 (Razor Pages)
- Entity Framework Core (SQLite)
- ASP.NET Identity
- Bootstrap 5.3.0
- RESTful API

## Getting Started
1. Clone the repository
2. Open in Visual Studio or VS Code
3. Run database migrations
4. Start the application (`dotnet run`)
5. Access the admin interface at `/Admin` (default admin: `admin@wtamu.edu` / `Admin@123!`)
