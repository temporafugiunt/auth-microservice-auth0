# auth-microservice-auth0

A simple microservice which provides Auth0 Authentication for a series of Microservice Applications in a Demo Kubernetes Cluster implemented to show Istio traffic routing capabilities.

This application provides 3 Services:

## Authentication

An unauthenticated user will be redirected to the auth0 domain setup in the application's cofiguration.

## Environment Authorization and Redirection

An authenticated user redirected back from Auth0 will be examined for an environment claim in the Authorization token, and the proper cookie will be set representing the proper environment for the user.

The user will then be redirected to the default microservice URI, and Istio will route the user to the version in the proper environment based upon their environment request header value.

## Logout

An authenticated user who goes to the default page will be given the chance to logout and be redirected back to the Auth0 domain for authentication with a different user.
