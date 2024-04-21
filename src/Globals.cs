global using System;
global using System.Linq;
global using System.Collections.Generic;
global using System.Runtime.CompilerServices;
global using System.Reflection;global using UnityEngine;
global using static System.Reflection.BindingFlags;
global using Random = UnityEngine.Random;
global using Color = UnityEngine.Color;
global using RWCustom;
global using MoreSlugcats;
global using DevInterface;
global using BepInEx;
global using BepInEx.Logging;
global using Fisobs.Core;
global using Fisobs.Creatures;
global using Fisobs.Sandbox;

global using Hailstorm;
global using DrainMites;
using System.Security.Permissions;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]