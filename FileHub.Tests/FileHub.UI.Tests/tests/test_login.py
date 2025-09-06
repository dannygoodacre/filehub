from playwright.sync_api import Page
from testdata import USERNAME, PASSWORD


def test_redirect_to_login(page: Page):
    page.goto("localhost")

    assert page.url.endswith("/login")


def test_login_success(page: Page):
    page.goto("localhost/login")

    page.get_by_role("textbox", name="username").fill(USERNAME)
    page.get_by_role("textbox", name="password").fill(PASSWORD)

    page.get_by_role("button", name="Log in").click()

    alert = page.get_by_role("alert")
    alert.wait_for(state="visible")
    
    assert alert.is_visible()

    assert alert.inner_text().strip() == "Login successful"


def test_login_failure(page: Page):
    page.goto("localhost/login")

    page.get_by_role("textbox", name="username").fill(USERNAME)
    page.get_by_role("textbox", name="password").fill("incorrect-password")

    page.get_by_role("button", name="Log in").click()
    
    alert = page.get_by_role("alert")
    alert.wait_for(state="visible")

    assert alert.is_visible()

    assert alert.inner_text().strip() == "Login failed"
