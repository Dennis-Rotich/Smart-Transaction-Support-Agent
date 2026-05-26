# Pesapal Webhook Integration Guide
Version: 1.0
To receive IPN (Instant Payment Notifications), ensure your endpoint returns a 200 OK status. 
If Pesapal does not receive a 200 OK, it will retry the webhook 4 times over 24 hours.