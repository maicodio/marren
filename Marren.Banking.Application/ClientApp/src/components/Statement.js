import React, { Component } from 'react';
import { Table } from 'reactstrap';
import session from './Session';

export class Statement extends Component {
  static displayName = Statement.name;

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
    const statement = await session.getStatement(+this.props.steps['account-statement'].value);

    if (statement.ok) {
      const data = statement.data.map(x => {
        const dateFull = session.formatIsoDateStr(x.date);
        return {
          datex: x.date,
          date: dateFull.substr(0, 10),
          time: dateFull.substr(10),
          type: x.type,
          value: x.value.toFixed(2).toString().replace('.', ','),
          balance: x.balance.toFixed(2).toString().replace('.', ',')
        }
      });
      this.setState({ statement: data, loading: false, error: null }, () => {
        this.props.triggerNextStep();
      });
    } else {

      let error = statement.message;
      if (statement.errors && statement.errors.length) {
        error = statement.errors[0].message;
      }

      this.setState({ statement: 0, loading: false, error }, () => {
        this.props.triggerNextStep();
      });
    }
  }

  render() {

    if (this.state.loading) {
      return (<div className="statement-msg">Carregando...</div>)
    }

    if (this.state.error) {
      return (<div className="statement-msg">{this.state.error}</div>)
    }
    var dataList = session.groupBy(this.state.statement, x => x.date);
    var last = this.state.statement[this.state.statement.length-1];
    return (
      <div className="statement">
        <h3>Extrato</h3>
        <Table>
          <thead>
            <tr>
              <th>Hora</th>
              <th>Tipo</th>
              <th>Valor</th>
            </tr>
          </thead>
          <tbody>
            {dataList.map(x =>
              <React.Fragment key={'trSaldo'+ x.groupKey.toString()}>
                <tr key={'trSaldo'+ x.groupKey.toString()}><td colSpan="3">Data: {x.groupKey}</td></tr>
                {x.values.map(v =>
                  <tr key={ v.datex + v.time + v.type}>
                    <td>{v.time}</td>
                    <td>{v.type}</td>
                    <td>{v.type == 'Saldo' ? v.balance : v.value}</td>
                  </tr>
                )}
              </React.Fragment>
            )}
          </tbody>
          <tfoot>
              <tr>
                <th colSpan="3">Saldo final: {last.balance}</th>
              </tr>
            </tfoot>
        </Table>
      </div>
    );
  }
}
