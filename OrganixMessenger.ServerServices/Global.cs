﻿global using Microsoft.AspNetCore.Http;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.Tokens;
global using OrganixMessenger.ServerData;
global using OrganixMessenger.ServerConfigurations;
global using OrganixMessenger.ServerModels.RefreshTokenModel;
global using OrganixMessenger.ServerServices.EmailServices;
global using OrganixMessenger.ServerServices.HttpContextServices;
global using OrganixMessenger.ServerServices.Repositories;
global using OrganixMessenger.ServerServices.Repositories.RefreshTokenRepositories;
global using OrganixMessenger.ServerServices.Repositories.UserRepositories;
global using OrganixMessenger.Shared;
global using System.ComponentModel.DataAnnotations;
global using System.IdentityModel.Tokens.Jwt;
global using System.Linq.Expressions;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.Text;
global using MimeKit;
global using MailKit.Net.Smtp;
global using Serilog.Ui.Web.Authorization;
global using OrganixMessenger.ServerModels.FileModel;
global using Microsoft.Extensions.Logging;
global using System.Net;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Configuration;
global using HealthChecks.UI.Client;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Diagnostics.HealthChecks;
global using Microsoft.Extensions.Diagnostics.HealthChecks;
global using MimeKit.Text;
global using OrganixMessenger.ServerModels.MessageModel;
global using OrganixMessenger.ServerModels.MessengerEntityModels.ApplicationBotModel;
global using OrganixMessenger.ServerModels.BotCommandModel;
