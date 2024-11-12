import datetime
import re
from zoneinfo import ZoneInfo
import time


# pytz in required to work with correct conversion of CE(Standard)T and CE(Summer)T
# This enable to correctly subtract 1 or 2 hours depending on the time of the year to get the correct UTC time
from pytz import timezone
import pytz

class CustomDate:
    def __init__ (self, hour, minutes, seconds, day, month, year, tz = "Europe/Rome"):
        self.hour = hour
        self.minutes = minutes
        self.seconds = seconds
        self.day = day
        self.month = month
        self.year = year
        
        self.tz = pytz.timezone(tz)

        # must contemplate also the time zone
        self.date = datetime.datetime(year, month, day, hour, minutes, seconds)
    
    def secondsTimeZone(self):
        return -int(time.timezone)

    def addDays(self, days):
        self.date += datetime.timedelta(days=days)
        return self

    # epoch is generated as the number of seconds since 1st January 1970 (This is the Unix epoch)
    # It is used as <iat> and <exp> fields in the JWT token
    def epoch(self):
        utc_date = self.tz.localize(self.date).astimezone(pytz.utc)
        return int(utc_date.timestamp())
    
    def parse(str):
        match_pattern = r"^(?P<day>[0-9]+)-(?P<month>[0-9]+)-(?P<year>[0-9]+)T(?P<hour>[0-9]+):(?P<minutes>[0-9]+):(?P<seconds>[0-9]+)$"
        match = re.match(match_pattern, str)
        if match is None:
            return None
        tz = "Europe/Rome"
        return CustomDate(
            int(match.group("hour")),
            int(match.group("minutes")),
            int(match.group("seconds")),
            int(match.group("day")),
            int(match.group("month")),
            int(match.group("year")),
            tz
        )
    
    def toISODate(self):
        return self.date.isoformat()
    
    def toDateOnly(self):
        return self.date.date()
    
    def __str__(self):
        return f"{self.date}"
    
