import React, { Component } from 'react';
import { Redirect } from 'react-router-dom';
import ChatBot from 'react-simple-chatbot';
import { Balance } from './Balance';
import { Login } from './Login';
import session from './Session';
import { Statement } from './Statement';
import { Withdraw } from './Withdraw';

export class Home extends Component {
  static displayName = Home.name;

  constructor(props) {
    super(props);

    this.state = { redirect: null };
  }

  end() {
    this.setState({redirect: '/register'})
  }

  render() {

    if (this.state.redirect) {
      return <Redirect to={this.state.redirect} />
    }

    return (
      <ChatBot
        headerTitle="Marren Account"
        placeholder="Digite um texto"
        width="100%"
        handleEnd={e=>this.end()}
        steps={[
          {
            id: 'welcome',
            message: 'Bem-vindo ao Marren! Como gostaria de entrar?',
            trigger: 'enter-options'
          },
          {
            id: 'enter-options',
            options: [
              { value: 1, label: 'Login', trigger: 'login' },
              { value: 2, label: 'Nova Conta', trigger: 'register-welcome' },
            ],
          },
          {
            id: 'login',
            asMessage: true,
            component: (<Login />),
            waitAction: true
          },
          {
            id: 'login-welcome',
            message: () => 'Bem vindo à sua conta, ' + localStorage.getItem('name') + '!',
            trigger: 'account-options'
          },
          {
            id: 'account-options',
            message: 'Como posso te ajudar?',
            trigger: 'account-options-value'
          },
          {
            id: 'account-options-value',
            options: [
              { value: 1, label: 'Saldo', trigger: 'account-balance' },
              { value: 2, label: 'Extrato', trigger: 'account-statement' },
              { value: 3, label: 'Depósito', trigger: 'account-deposit' },
              { value: 4, label: 'Saque', trigger: 'account-withdraw' },
              { value: 5, label: 'Sair', trigger: 'welcome' },
            ],
          },
          {
            id: 'account-balance',
            asMessage: true,
            component: (<Balance />),
            waitAction: true,
            trigger: 'account-options'
          },
          {
            id: 'account-deposit',
            message: 'Oba! Vamos depositar quanto?',
            trigger: 'account-deposit-value'
          },
          {
            id: 'account-deposit-value',
            user: true,
            placeholder: 'Digite o valor do depósito',
            validator: value => {
              if (!/^\s*\-?\s*[0-9]{1,10}(,[0-9]{1,2})?\s*$/g.test(value)) {
                return 'Não é um número válido.';
              }
              return true;
            },
            trigger: 'account-deposit-balance'
          },
          {
            id: 'account-deposit-balance',
            asMessage: true,
            waitAction: true,
            component: (<Balance action="deposit" />),
            trigger: 'account-options'
          },
          {
            id: 'account-withdraw',
            message: 'Ok! Quanto você precisa sacar?',
            trigger: 'account-withdraw-value'
          },
          {
            id: 'account-withdraw-value',
            user: true,
            placeholder: 'Digite o valor do saque',
            validator: value => {
              if (isNaN(value)) {
                return 'Não é um número válido.';
              }
              return true;
            },
            trigger: 'account-withdraw-balance'
          },
          {
            id: 'account-withdraw-balance',
            asMessage: true,
            waitAction: true,
            component: (<Withdraw />),
            trigger: 'account-options'
          },
          {
            id: 'account-statement',
            options: [
              { value: 7, label: '7 dias', trigger: 'account-statement-view' },
              { value: 15, label: '15 dias', trigger: 'account-statement-view' },
              { value: 30, label: '30 dias', trigger: 'account-statement-view' },
              { value: 60, label: '60 dias', trigger: 'account-statement-view' },
            ],
          },
          {
            id: 'account-statement-view',
            asMessage: false,
            waitAction: true,
            component: (<Statement />),
            trigger: 'account-options'
          },
          {
            id: 'register-welcome',
            message: 'Beleza, vamos abrir uma conta! Você será redirecionado para a tela de cadastro. OK?',
            trigger: 'register-options'
          },
          {
            id: 'register-options',
            options: [
              { value: 1, label: 'Sim', trigger: 'register-end' },
              { value: 2, label: 'Não', trigger: 'welcome' },
            ],
          },
          {
            id: 'register-end',
            message: 'Valeu! Até daqui a pouco!',
            end: true
          },
        ]}
      />
    );
  }
}
