import { Environment } from '@abp/ng.core';

const baseUrl = 'https://treadsnow.com.cn';

const oAuthConfig = {
  issuer: 'https://treadsnow.com.cn:5001/',
  redirectUri: baseUrl,
  clientId: 'TreadSnow_App',
  responseType: 'code',
  scope: 'offline_access TreadSnow',
  requireHttps: true,
};

export const environment = {
  production: true,
  application: {
    baseUrl,
    name: 'TreadSnow',
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'https://treadsnow.com.cn:5001',
      rootNamespace: 'TreadSnow',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
  remoteEnv: {
    url: '',
    mergeStrategy: 'deepmerge'
  }
} as Environment;
