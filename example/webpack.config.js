var path = require("path");
var Notifier = require('webpack-notifier');

module.exports = {
  entry: './src/index',
  output: {
    path: path.resolve(__dirname,"build"),
    publicPath: '/',
    filename: 'bundle.js'
  },
  resolve: {
    extensions: ['', '.js', '.jsx'],
    modulesDirectories: [
      'src',
      'node_modules'
    ]
  },
  module: {
    loaders: [
      {
        test: /\.(js|jsx)$/,
        loader: 'babel-loader',
        options: {
          cacheDirectory: true
        },
        query: {
          presets: ['es2015', 'react']
        }
      }
    ]
  },
  plugins: [
    new Notifier({
      alwaysNotify: true
    })
  ],
  devServer: {
    hot: true
  }
};
