global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using Xunit;
global using Moq;
global using FluentAssertions;
global using CommunityToolkit.Mvvm.ComponentModel;
global using CommunityToolkit.Mvvm.Input;
global using CommunityToolkit.Maui;
global using Microsoft.Maui.ApplicationModel;
global using Microsoft.Maui.Controls;

// Domain Layer (from Meow.Domain project reference)
global using Meow.Domain.Entities;
global using Meow.Domain.Interfaces;

// Core Layer (from Meow.Core project reference)
global using Meow.Core.Common;
global using Meow.Core.UseCases;

// Presentation Layer (linked files)
global using Meow.Presentation.ViewModels;
