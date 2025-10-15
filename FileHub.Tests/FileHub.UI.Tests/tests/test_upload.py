from playwright.sync_api import Page, expect
from pathlib import Path
from testdata import USERNAME, PASSWORD


def test_upload(page: Page):
    # Login
    page.goto('localhost/login')
    
    page.get_by_role('textbox', name='username').fill(USERNAME)
    page.locator('input[type="password"]').fill(PASSWORD)

    page.get_by_role('button', name='Login').click()
    
    # Upload file
    filepath = '../FileHub.Tests.Seeder/TestFiles/image1.png'
    name = 'Automation test file'
    tags = ['AT tag 1', 'AT tag 2']

    page.get_by_role('link', name='Upload').click()
    
    file = Path(filepath).resolve()

    page.set_input_files('input[type="file"]', file)

    page.get_by_role('textbox', name='Name').fill(name)
    
    page.get_by_role('textbox', name='Tag').fill(tags[0])
    page.get_by_role('button', name='Add').click()

    page.get_by_role('textbox', name='Tag').fill(tags[1])
    page.get_by_role('button', name='Add').click()

    page.get_by_role('button', name='Upload').click()

    expect(page.get_by_text('File uploaded successfully!')).to_be_visible()

    # Go to home page
    page.get_by_role('link', name='Home').click()

    expect(page.get_by_text(name)).to_be_visible()
    expect(page.get_by_text(tags[0])).to_be_visible()
    expect(page.get_by_text(tags[1])).to_be_visible()
