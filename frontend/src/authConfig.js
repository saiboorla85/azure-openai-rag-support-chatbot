export const msalConfig = {
  auth: {
    clientId: "97c33ac9-8576-486f-97f1-6eb44072271e",
    authority: "https://login.microsoftonline.com/80334c16-01d4-4ae2-bf04-7275c051670d",
    redirectUri: "http://localhost:5173",
    postLogoutRedirectUri: "http://localhost:5173"
  },
  cache: {
    cacheLocation: "sessionStorage",
    storeAuthStateInCookie: false
  }
};

export const loginRequest = {
   scopes: ["api://be7c5eca-996f-4a2d-b10d-bc2a0e664d6a/access_as_user"]
};