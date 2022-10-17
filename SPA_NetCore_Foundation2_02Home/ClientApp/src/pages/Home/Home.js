import React, { Component } from 'react';
import $ from 'jquery';
import { Link } from 'react-router-dom';


import parse from 'html-react-parser';
import { replace } from 'lodash';


import GlobalStatic from '@/Global/GlobalStatic.js';

export default class Home extends Component
{


    constructor()
    {
        super();
    }

    render()
    {
        return (
            <div className="Test">
                Home
            </div>
        );
    }//end render
}

