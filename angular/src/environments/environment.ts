 import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'https://localhost:5000/',
  redirectUri: baseUrl,
  clientId: 'TreadSnow_App',
  responseType: 'code',
  scope: 'offline_access TreadSnow',
  requireHttps: true,
};

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'TreadSnow',
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'https://localhost:44312',
      rootNamespace: 'TreadSnow',
    },
    AbpAccountPublic: {
      url: 'https://localhost:5000/',
      rootNamespace: 'AbpAccountPublic',
    },
  },
  remoteEnv: {
    url: '',
    mergeStrategy: 'deepmerge'
  }
} as Environment;
