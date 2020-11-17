import React, { Component } from 'react';
import session from './Session';

export class Balance extends Component {
  static displayName = Balance.name;

  constructor(props) {
    super(props);
    this.state = {
      balance: 0,
      message: null,
      loading: true,
      trigger: false,
    };
  }

  async componentDidMount() {
    let balance = null;
    console.log(this.props);
    if (this.props.action == 'deposit') {
      const valor = +this.props.steps['account-deposit-value'].value.trim().replace(',','.');
      balance = await session.deposit(valor);
    } else {
      balance = await session.getBalace();
    }

    if (balance.ok) {
      this.setState({balance: balance.data, loading: false, error: null },() => {
        this.props.triggerNextStep();
      });
    } else {

      let error = balance.message;
      console.log(balance);
      if (balance.errors && balance.errors.length) {
        error = balance.errors[0].message;
      }

      this.setState({balance: 0, loading: false,error},() => {
        this.props.triggerNextStep();
      });
    }
  }

  render () {

    if (this.state.loading) {
      return (<div>Carregando...</div>)   
    }

    if (this.state.error) {
      return (<div>{this.state.error}</div>)   
    }

    return (
      <div>
        {this.props.action == 'deposit' ? <div>Depósito realizado.</div> : null}
        <div>Seu saldo atual é {this.state.balance.toFixed(2).toString().replace('.', ',')}.</div>
        {this.state.balance < 0 ? <div>Você está usando o cheque especial. Fale com seu gerente sobre melhores opções de financiamento.</div> : null}
      </div>
    );
  }
}
