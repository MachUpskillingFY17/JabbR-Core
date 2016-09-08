import os

username = os.environ["SAUCE_USERNAME"]
access_key = os.environ["SAUCE_ACCESS_KEY"]

hub_url = "%s:%s@localhost:4445" % (username, access_key)
hub_url2 = "%s:%s@ondemand.saucelabs.com:80" % (username, acces_key)

# desired_cap = {
#     'platform': "Mac OS X 10.9",
#     'browserName': "chrome",
#     'version': "31",
# }

caps = {'browserName': "chrome"}
caps['platform'] = "Windows 10"
caps['version'] = "52.0"

driver = webdriver.Remote(desired_capabilities=caps, command_executor="http://%s/wd/hub" % hub_url)

# driver = webdriver.Remote(command_executor='http://%s/wd/hub' % hub_url2, desired_capabilities=caps)