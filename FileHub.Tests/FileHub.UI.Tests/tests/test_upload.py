from playwright.sync_api import Page, expect
from pathlib import Path
from testdata import USERNAME, PASSWORD


def test_upload(page: Page):
    page.goto("localhost/login")
    
    page.get_by_role("textbox", name="username").fill(USERNAME)
    page.get_by_role("textbox", name="password").fill(PASSWORD)

    page.get_by_role("button", name="Log in").click()
    
    page.get_by_role("link", name="Upload").click()
    
    file = Path("../FileHub.Tests.Seeder/TestFiles/image1.png").resolve()

    page.set_input_files("input[type='file']", file)

    page.get_by_role("textbox", name="Name").fill("Automation test file")
    
    page.get_by_role("textbox", name="Tag").fill("AT tag 1")
    page.get_by_role("button", name="Add").click()

    page.get_by_role("textbox", name="Tag").fill("AT tag 2")
    page.get_by_role("button", name="Add").click()

    page.get_by_role("button", name="Upload File").click()

    expect(page.get_by_text("File uploaded successfully!")).to_be_visible()
