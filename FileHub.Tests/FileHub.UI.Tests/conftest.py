import pytest
from playwright.sync_api import sync_playwright, Browser


@pytest.fixture(scope="session")
def browser():
    with sync_playwright() as p:

        browser = p.chromium.launch(headless=False)

        yield browser

        browser.close()


@pytest.fixture
def page(browser: Browser):
    context = browser.new_context(ignore_https_errors=True)
    page = context.new_page()

    yield page
    
    page.close()
