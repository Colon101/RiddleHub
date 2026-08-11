#!/usr/bin/env python3
"""Offline regression checks for RiddleHub's security invariants."""

from pathlib import Path
import unittest
import xml.etree.ElementTree as ET


ROOT = Path(__file__).resolve().parents[1]
WEB_ROOT = ROOT / "RiddleHub"


def read(relative_path: str) -> str:
    return (ROOT / relative_path).read_text(encoding="utf-8-sig")


class SecurityRegressionTests(unittest.TestCase):
    def test_database_and_connection_diagnostic_are_not_shipped(self) -> None:
        removed = (
            "RiddleHub/App_Data/db.mdf",
            "RiddleHub/App_Data/db_log.ldf",
            "RiddleHub/connstring.aspx",
            "RiddleHub/connstring.aspx.cs",
            "RiddleHub/connstring.aspx.designer.cs",
        )
        for relative_path in removed:
            self.assertFalse((ROOT / relative_path).exists(), relative_path)

        project = read("RiddleHub/RiddleHub.csproj")
        for relative_path in ("db.mdf", "db_log.ldf", "connstring.aspx"):
            self.assertNotIn(relative_path, project)

    def test_passwords_use_pbkdf2_and_legacy_rows_require_reset(self) -> None:
        password_security = read("RiddleHub/PasswordSecurity.cs")
        helper = read("RiddleHub/App_Data/Helper.cs")
        login = read("RiddleHub/login.aspx.cs")
        signup = read("RiddleHub/signup.aspx.cs")
        reset = read("RiddleHub/resetpassword.aspx.cs")

        self.assertIn('FormatName = "pbkdf2-sha256"', password_security)
        self.assertIn("Rfc2898DeriveBytes", password_security)
        self.assertIn("HashAlgorithmName.SHA256", password_security)
        self.assertIn("IterationCount = 150000", password_security)
        self.assertIn("PasswordSecurity.HashPassword(password)", signup)
        self.assertIn("LegacyPlaintextSuccess", login)
        self.assertIn("[password_reset_required] = 1", login)
        self.assertIn("pending_password_reset_expires", login)
        self.assertIn("[password_reset_required] = 0", reset)
        self.assertIn("WHERE [password] NOT LIKE N'pbkdf2-sha256$%'", helper)
        self.assertNotIn('Session["password"]', "\n".join(
            path.read_text(encoding="utf-8-sig")
            for path in WEB_ROOT.rglob("*.cs")
        ))

    def test_session_version_revokes_old_sessions(self) -> None:
        utility = read("RiddleHub/SimpleUtilFunctions.cs")
        account = read("RiddleHub/account.aspx.cs")
        reset = read("RiddleHub/resetpassword.aspx.cs")
        self.assertIn("AND [session_version] = @SessionVersion", utility)
        self.assertIn('session["session_version"] = sessionVersion', utility)
        self.assertGreaterEqual(account.count("[session_version] + 1"), 1)
        self.assertIn("[session_version] = [session_version] + 1", reset)
        self.assertIn("session.Abandon()", utility)

    def test_all_mutating_handlers_enforce_csrf(self) -> None:
        handlers = (
            "RiddleHub/login.aspx.cs",
            "RiddleHub/signup.aspx.cs",
            "RiddleHub/resetpassword.aspx.cs",
            "RiddleHub/account.aspx.cs",
            "RiddleHub/create.aspx.cs",
            "RiddleHub/editriddle.aspx.cs",
            "RiddleHub/admin.aspx.cs",
            "RiddleHub/signout.aspx.cs",
        )
        for handler in handlers:
            self.assertIn("CsrfProtection.RejectInvalidPost", read(handler), handler)

        forms = (
            "RiddleHub/login.aspx",
            "RiddleHub/signup.aspx",
            "RiddleHub/resetpassword.aspx",
            "RiddleHub/account.aspx",
            "RiddleHub/create.aspx",
            "RiddleHub/admin.aspx",
            "RiddleHub/signout.aspx",
        )
        for page in forms:
            markup = read(page)
            self.assertEqual(markup.count("<form"), markup.count('name="csrf_token"'), page)

        self.assertIn('body: "action=delete&id="', read("RiddleHub/my.aspx"))
        self.assertIn('csrf_token=" + encodeURIComponent(csrfToken)', read("RiddleHub/my.aspx"))

    def test_authentication_attempts_are_rate_limited(self) -> None:
        controls = read("RiddleHub/SecurityControls.cs")
        self.assertIn("PerAddressLimit", controls)
        self.assertIn("PerIdentityLimit", controls)
        self.assertIn("MaximumTrackedKeys", controls)
        for handler in ("RiddleHub/login.aspx.cs", "RiddleHub/signup.aspx.cs", "RiddleHub/admin.aspx.cs"):
            source = read(handler)
            self.assertIn("AuthRateLimiter.IsAllowed", source, handler)
            self.assertIn("AuthRateLimiter.RecordFailure", source, handler)
            self.assertIn("StatusCode = 429", source, handler)

    def test_admin_secret_and_dotenv_fail_closed(self) -> None:
        admin = read("RiddleHub/admin.aspx.cs")
        env_example = read(".env.example")
        self.assertIn("IsAdminSessionAuthenticated", admin)
        self.assertIn("CreatePasswordFingerprint", admin)
        self.assertIn("PasswordSecurity.FixedTimeEqualsUtf8", admin)
        self.assertIn("IsOutsideDirectory(path, appRoot)", admin)
        self.assertNotIn('Path.Combine(appRoot, ".env")', admin)
        self.assertIn("RIDDLEHUB_ADMIN_PASSWORD=\n", env_example)
        self.assertIn("RIDDLEHUB_SQL_PASSWORD=\n", env_example)

    def test_browser_routes_and_messages_are_allowlisted(self) -> None:
        index = read("RiddleHub/index.js")
        iframe = read("RiddleHub/iframe.js")
        combined = index + "\n" + iframe
        self.assertIn("const VALID_PAGES", index)
        self.assertIn("/^edit([1-9][0-9]*)$/", index)
        self.assertIn("event.origin !== window.location.origin", index)
        self.assertIn("event.source !== contentElement.contentWindow", index)
        self.assertIn("event.origin !== PARENT_ORIGIN", iframe)
        self.assertIn("event.source !== parent", iframe)
        self.assertIn("isAllowedOutboundMessage", iframe)
        self.assertNotIn('postMessage("THEME" + themeName, "*")', combined)
        self.assertNotIn('parent.postMessage(message, "*")', combined)
        self.assertNotIn("contentElement.src = window.location.hash", index)

    def test_development_listeners_and_release_tls_are_hardened(self) -> None:
        run_script = read("scripts/run-linux.sh")
        apache = read("scripts/apache-riddlehub.conf.example")
        web_config = read("RiddleHub/Web.config")
        release = read("RiddleHub/Web.Release.config")

        self.assertIn('-p "127.0.0.1:$RIDDLEHUB_SQL_PORT:1433"', run_script)
        self.assertIn('RIDDLEHUB_BIND_ADDRESS:-127.0.0.1', run_script)
        self.assertIn('--address "$RIDDLEHUB_BIND_ADDRESS"', run_script)
        self.assertIn("Listen 127.0.0.1:8080", apache)
        self.assertIn('<FilesMatch "^\\.">', apache)
        self.assertIn('segment=".env"', web_config)
        self.assertNotIn("Encrypt=False", web_config)
        self.assertIn('requireSSL="true"', release)
        self.assertIn("Strict-Transport-Security", release)

    def test_configuration_and_project_files_are_well_formed_xml(self) -> None:
        for relative_path in (
            "RiddleHub/RiddleHub.csproj",
            "RiddleHub/Web.config",
            "RiddleHub/Web.Debug.config",
            "RiddleHub/Web.Release.config",
        ):
            ET.parse(ROOT / relative_path)


if __name__ == "__main__":
    unittest.main(verbosity=2)
