const path = require('path');
const webpack = require('webpack');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const CssMinimizerPlugin = require('css-minimizer-webpack-plugin');
const TerserPlugin = require('terser-webpack-plugin');

// bundle versions, written as a header comment in the output files; bump when the bundle changes
const versions = {
    'VA009.all': '3.0.1.0',  // JS
    'VA009': '3.0.1.0'      // CSS
};

// the CSS entry also makes an empty JS chunk; it is not needed, so it is not emitted
class RemoveStyleEntryScript {
    apply(compiler) {
        compiler.hooks.thisCompilation.tap('RemoveStyleEntryScript', compilation => {
            compilation.hooks.processAssets.tap(
                { name: 'RemoveStyleEntryScript', stage: webpack.Compilation.PROCESS_ASSETS_STAGE_SUMMARIZE },
                () => {
                    compilation.deleteAsset('VA009.min.js');
                });
        });
    }
}

module.exports = {
    //mode: 'development', // for debuggin
    mode: 'production',
    // devtool is left to webpack's default: development = 'eval' (every source file shows separately in the
    // browser's DevTools under webpack://, readable), production = none
    entry: {
        'VA009.all': './Areas/VA009/Scripts/src/VA009js.js',
        'VA009': './Areas/VA009/Contents/src/VA009css.css'
    },
    output: {
        filename: '[name].min.js',
        path: path.resolve(__dirname, 'Areas/VA009/Scripts/dist'),
        // only Scripts/dist is cleaned, never the source folders
        clean: true
    },
    resolve: {
        extensions: ['.js'],
    },
    module: {
        rules: [
            {
                test: /\.js$/,
                exclude: /node_modules/,
                use: {
                    loader: 'babel-loader'
                },
            },
            {
                test: /\.css$/,
                use: [MiniCssExtractPlugin.loader, {
                    loader: 'css-loader',
                    options: {
                        // keep image paths as written; the bundle sits in Contents next to the source files
                        url: false,
                    }
                }],
            }
        ]
    },
    plugins: [
        new MiniCssExtractPlugin({
            // relative to Scripts/dist: Areas/VA009/Contents/VA009.all.min.css (path used by VA009AreaRegistration)
            filename: '../../Contents/[name].all.min.css',
        }),
        new RemoveStyleEntryScript(),
        new webpack.BannerPlugin({
            banner: '/*Version ' + versions['VA009.all'] + '*/',
            raw: true,
            test: /\.js$/,
            stage: webpack.Compilation.PROCESS_ASSETS_STAGE_REPORT
        }),
        new webpack.BannerPlugin({
            banner: '/***version ' + versions['VA009'] + '*/',
            raw: true,
            test: /\.css$/,
            stage: webpack.Compilation.PROCESS_ASSETS_STAGE_REPORT
        }),
    ],
    optimization: {
        minimize: true, // as in VAS; in development the module code inside eval() stays readable
        minimizer: [
            new CssMinimizerPlugin(), // Minify CSS
            new TerserPlugin({
                terserOptions: {
                    format: {
                        comments: false,
                    },
                },
                extractComments: false,
            }), // Minify JavaScript
        ]
    },
    performance: {
        // PaymentForm.js alone is ~800 KB of source
        hints: false
    }
};
