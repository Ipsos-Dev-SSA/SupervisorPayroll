﻿using CallCentreFollowUps.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using CallCentreFollowUps.Controllers;

public class UserController : Controller
{
    private CallCentreTrackerEntities1 _context;

    public UserController()
    {
        _context = new CallCentreTrackerEntities1();
    }

    public ActionResult Index()
    {
        var users = _context.aspnet_Users.ToList();
        return View(users);
    }

    public ActionResult Create()
    {
        ViewBag.Roles = _context.aspnet_Roles
            .Select(r => new SelectListItem
            {
                Value = r.RoleName,
                Text = r.RoleName
            }).ToList();

        ViewBag.Countries = _context.Countries
            .Select(c => new SelectListItem
            {
                Value = c.CountryID.ToString(),
                Text = c.CountryName
            }).ToList();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(NewUserModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = _context.aspnet_Roles
                .Select(r => new SelectListItem
                {
                    Value = r.RoleName,
                    Text = r.RoleName
                }).ToList();

            ViewBag.Countries = _context.Countries
                .Select(c => new SelectListItem
                {
                    Value = c.CountryID.ToString(),
                    Text = c.CountryName
                }).ToList();

            return View(model);
        }

        string userName = $"{model.FirstName}.{model.LastName}";
        Guid userId;

        var existingUser = _context.aspnet_Membership
            .FirstOrDefault(m => m.Email == model.EmailAddress);

        if (existingUser != null)
        {
            // User already exists
            userId = _context.aspnet_Users
                .Where(u => u.UserName == userName)
                .Select(u => u.UserId)
                .FirstOrDefault();
        }
        else
        {
            // Create new user
            var application = _context.aspnet_Applications
                .FirstOrDefault(a => a.ApplicationName == "CallCentreFollowUps");

            if (application == null)
            {
                ModelState.AddModelError("", "Application not found.");
                return View(model);
            }

            userId = Guid.NewGuid();

            // Insert into aspnet_Users
            _context.Database.ExecuteSqlCommand(@"
                INSERT INTO dbo.aspnet_Users
                (ApplicationId, UserId, UserName, LoweredUserName, MobileAlias, IsAnonymous, LastActivityDate, CountryID)
                VALUES (@p0, @p1, @p2, @p3, NULL, 0, GETDATE(), @p4)",
                application.ApplicationId, userId, userName, userName.ToLower(), model.CountryID);

            // Insert into aspnet_Membership
            _context.Database.ExecuteSqlCommand(@"
                INSERT INTO dbo.aspnet_Membership
                (ApplicationId, UserId, Password, PasswordFormat, PasswordSalt, Email, LoweredEmail, PasswordQuestion, PasswordAnswer, IsApproved,
                 IsLockedOut, CreateDate, LastLoginDate, LastPasswordChangedDate, LastLockoutDate, FailedPasswordAttemptCount, FailedPasswordAttemptWindowStart,
                 FailedPasswordAnswerAttemptCount, FailedPasswordAnswerAttemptWindowStart, Comment)
                VALUES (@p0, @p1, @p2, 0, '', @p3, @p4, 'My name and surname', @p5, 1,
                        0, GETDATE(), GETDATE(), GETDATE(), GETDATE()-1, 0, GETDATE()-1, 0, GETDATE()-1, 'Created by Code')",
                application.ApplicationId, userId, model.Password, model.EmailAddress, model.EmailAddress.ToLower(), $"{model.FirstName} {model.LastName}");
        }

        // Assign role if not already assigned
        var roleId = _context.aspnet_Roles
            .Where(r => r.RoleName == model.RoleName)
            .Select(r => r.RoleId)
            .FirstOrDefault();

        if (roleId == Guid.Empty)
        {
            ModelState.AddModelError("", "Role not found.");
            return View(model);
        }

        bool roleExists = _context.vw_aspnet_UsersInRoles
            .Any(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (!roleExists)
        {
            _context.Database.ExecuteSqlCommand(@"
                INSERT INTO dbo.aspnet_UsersInRoles (UserId, RoleId)
                VALUES (@p0, @p1)", userId, roleId);
        }

        return RedirectToAction("Index");
    }

    // Method to show the password update form
    public ActionResult UpdatePassword()
    {
        return View();
    }

    // POST method to handle password update
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult UpdatePassword(UpdatePasswordModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Check if the old password is correct
        var user = _context.aspnet_Membership
            .FirstOrDefault(u => u.UserId == _context.aspnet_Users
            .Where(x => x.UserName == User.Identity.Name)
            .Select(x => x.UserId)
            .FirstOrDefault());

        if (user == null || !user.Password.Equals(model.OldPassword))
        {
            ModelState.AddModelError("", "The old password is incorrect.");
            TempData["Message"] = "error";
            TempData["MessageContent"] = "The old password is incorrect.";
            return View(model);
        }

        // Update the password in the database
        user.Password = model.NewPassword;
        user.LastPasswordChangedDate = DateTime.Now;

        _context.SaveChanges();

        // On success
        TempData["Message"] = "success";
        TempData["MessageContent"] = "Password updated successfully.";

        return RedirectToAction("LandingPage","Home");
    }



    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _context.Dispose();
        }
        base.Dispose(disposing);
    }
}


