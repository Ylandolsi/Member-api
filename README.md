# MemberOnly API

MemberOnly API is a secure RESTful API built with .NET 9.0 that manages user authentication, content posts, and user actions in a members-only environment.

## Features

- Secure authentication using JWT tokens with refresh token rotation
- User registration and login
- User profile management
- Content posts creation and management
- Role-based authorization
- User action tracking

## API Endpoints

### Authentication

- `POST /api/User/register` - Register a new user
- `POST /api/User/login` - Login and receive JWT tokens
- `POST /api/Auth/refresh-token` - Refresh an expired access token
- `POST /api/User/logout` - Logout (invalidate refresh token)
- `GET /api/Auth/validate` - Validate a JWT token

### User Management

- `GET /api/User/{username}/info` - Get user information
- `GET /api/User/check-username` - Check if a username is available
- `GET /api/User/check-completion` - Check if a user has completed required actions
- `POST /api/User/complete-action` - Mark a user action as completed

### Posts

- `GET /api/Post/all` - Get all posts
- `GET /api/Post/user/{username}` - Get posts by username
- `GET /api/Post/{id}` - Get a specific post
- `GET /api/Post/myPosts` - Get posts of the authenticated user
- `POST /api/Post` - Create a new post
- `POST /api/Post/create` - Alternative endpoint to create a post
- `DELETE /api/Post/{id}` - Delete a post

## Security Notes

- Password requirements: 8+ characters with at least one uppercase letter, one lowercase letter, one number, and one special character
- JWTs expire in 5 minutes (configurable in appsettings)
- Refresh tokens expire in 5 minutes (configurable)

## Database Schema

The database contains the following main tables:

- `Users` - Store user information
- `Posts` - Store post content with foreign key to Users
- `RefreshTokens` - Track active refresh tokens for users
